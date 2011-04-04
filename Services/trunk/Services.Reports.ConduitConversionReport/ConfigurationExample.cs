//<?xml version="1.0" encoding="utf-8"?>
//<configuration>
//    <configSections>
//        <section name="SmtpConnection" type="System.Configuration.SingleTagSectionHandler" />
//        <section name="edge.services" type="Easynet.Edge.Core.Configuration.ServicesSection, Edge.Core" />
//    </configSections>

//    <runtime>
//        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
//            <probing privatePath="Services" />
//        </assemblyBinding>
//    </runtime>

//    <!-- ======================================================== -->
//    <!--						App Settings					  -->
//    <!-- ======================================================== -->
//    <appSettings>
//        <add key="DebugMode" value="true" />

//        <!--Core-->
//        <add key="Easynet.Edge.Core.Data.DataManager.Connection.String" value="Data Source=localhost;Initial Catalog=Seperia;Integrated Security=false;User ID=edge;PWD=edgebi!" />
//        <add key="Easynet.Edge.Core.Data.DataManager.Connection.Timeout" value="1000000" />
//        <add key="Easynet.Edge.Core.Services.Service.BaseListeningUrl" value="net.tcp://localhost:3535/{0}"/>
//        <add key="Easynet.Edge.Core.Services.Service.AssemblyDirectory" value="Services"/>
//        <add key="Easynet.Edge.Core.Configuration.ServiceElement.DefaultMaxExecutionTime" value="00:40:00"/>
//        <add key="Easynet.Edge.Core.Utilities.Log.LogName" value="easynet Edge"/>
//        <add key="Easynet.Edge.Core.Domain" value="seperia.hz"/>

//        <!--ScheduleManagement-->
//        <add key="Easynet.Edge.Services.ScheduleManagement.ScheduleBuilder.ResultsRoot" value="D:\Edge\Seperia Retrieved Files" />
//        <add key="Easynet.Edge.Services.ScheduleManagement.ScheduleTable.KeepAliveCheckTime" value="5" />

//        <!--BaseService-->
//        <add key="Easynet.Edge.Services.DataRetrieval.BaseService.SourceConnectionString" value="Data Source=localhost;Initial Catalog=Source;Integrated Security=false;User ID=edge;PWD=edgebi!" />
//        <add key="Easynet.Edge.Services.DataRetrieval.BaseService.ResultsRoot" value="D:\Edge\Seperia Retrieved Files" />

//    </appSettings>

//    <edge.services>


//        <!-- ======================================================== -->
//        <!--						Services						  -->
//        <!-- ======================================================== -->
//        <Services>

//            <!-- ======================================================== -->
//            <!--					EdgeServiceHost services			  -->
//            <!-- ======================================================== -->
//            <Service Name="ScheduleManagerAlert"
//                     Class="Easynet.Edge.Services.ScheduleManagement.ScheduleManagerService, Edge.Services.ScheduleManagement"
//                     />

//            <!-- ======================================================== -->
//            <!--					System Services						  -->
//            <!-- ======================================================== -->


//            <Service Name ="ReportByEmail"
//                    Class="Easynet.Edge.Services.Reports.ConduitConversionReport,Edge.Services.Reports.ConduitConversionReport"
//                    ConnectionTimeout="00:10:00"
//                    FileSavePath="D:\Edge\Reports\"
//                    Procedure="SP_GetConduitConversionReport(@Date:NVarchar)"
//                    param.day="20100101"
//                    >
//            </Service>
//        </Services>
//        <!-- ======================================================== -->
//        <!--					Accounts							  -->
//        <!-- ======================================================== -->
//        <Accounts>
//            <Account ID="95" Name="Conduit">
//                <Services>
//                    <Service Uses="ReportByEmail">
//                        <SchedulingRules  Overrides="true">
//                            <Rule ExactTimes="00:01" CalendarUnit="Day" MaxDeviation="23:59" />
//                        </SchedulingRules>
//                    </Service>
//                </Services>
//            </Account>
//        </Accounts>
//    </edge.services>

//    <!-- ======================================================== -->
//    <!--				Smtp configuration						  -->
//    <!-- ======================================================== -->
//    <SmtpConnection server="208.131.155.102" port="25" to="edge.alerts@seperia.com" from="alerts@seperia.com" user="alerts@seperia.com" pass="ALal1212"
//                        UseDefaultCredentials = "false"
//                        EnableSsl = "false"
//                  />
//    <!-- ======================================================== -->
//    <!--				WCF configuration						  -->
//    <!-- ======================================================== -->

//    <system.serviceModel>
//        <!--Server configuration-->
//        <services>
//            <service name="Easynet.Edge.Services.ScheduleManagement.ScheduleManagerService" behaviorConfiguration="behavior">
//                <endpoint binding="wsHttpBinding" bindingConfiguration="edgeServiceWebBinding" contract="Easynet.Edge.Core.Scheduling.IScheduleManager" address="http://localhost:27334/v2.1/ScheduleManagerAlert" />
//            </service>
//            <service name="Easynet.Edge.BusinessObjects.Services.GkManagerService" behaviorConfiguration="behavior">
//                <endpoint binding="netTcpBinding" bindingConfiguration="gkManagerBinding" contract="Easynet.Edge.Core.Data.IIdentityManager" address="net.tcp://localhost:3636/GkManager" />
//            </service>
//        </services>

//        <!--Client configuration 
//         Just copy&paste the <endpoint> elements from the <services> element-->
//        <client>
//            <endpoint binding="wsHttpBinding" bindingConfiguration="edgeServiceWebBinding" contract="Easynet.Edge.Core.Scheduling.IScheduleManager" address="http://localhost:27334/v2.1/ScheduleManagerAlert" />
//            <endpoint binding="netTcpBinding" bindingConfiguration="gkManagerBinding" contract="Easynet.Edge.Core.Data.IIdentityManager" address="net.tcp://localhost:3636/GkManager" />
//        </client>

//        <behaviors>
//            <serviceBehaviors>
//                <behavior name="behavior">
//                    <serviceMetadata httpGetEnabled="false" />
//                    <serviceDebug includeExceptionDetailInFaults="true" />
//                </behavior>
//            </serviceBehaviors>
//        </behaviors>

//        <bindings>
//            <netTcpBinding>
//                <binding name="edgeServiceCommBinding" portSharingEnabled="true" receiveTimeout="1:0:0" />
//                <binding name="gkManagerBinding" portSharingEnabled="false" receiveTimeout="0:2:0"/>
//            </netTcpBinding>
//            <wsHttpBinding>
//                <binding name="edgeServiceWebBinding" sendTimeout="0:10:00" receiveTimeout="0:10:00" />
//            </wsHttpBinding>
//        </bindings>

//    </system.serviceModel>

//</configuration>


