namespace HubSpotControl.Contract
{
    public class ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = $"{Root}/{Version}";

        public static class AssocaitedServ
        {
            //public const string GetAll = Base + "/flycardServs";
            //public const string Get = Base + "/flycardServs/{postid}";
            //public const string Create = Base + "/flycardServs";
            //public const string Update = Base + "/flycardServs/{postid}";
            //public const string Delete = Base + "/flycardServs/{postid}";

            public const string GetAllAssociationsBetweenObject = Base + "/AssocaitedServs/GetAssociationsList/{from_object}/{to_object}";
            public const string ReadData = Base + "/AssocaitedServs/ReadDataList";
            public const string DeleteDuplicateContact = Base + "/AssocaitedServs/DeleteDuplicateContactList";
            public const string MissMatchContact = Base + "/AssocaitedServs/MissMatchContactList";
            public const string AssociatedDeleteAllFromContact = Base + "/AssocaitedServs/AssociatedDeleteAllFromContactList";
            //public const string GetpopulationForPaymentReminder = Base + "/flycardServs/populationForPaymentReminder/{codeAction}/{maxDays}/{LogIdInforU}";

            public const string ArchiveInActiveData = Base + "/AssocaitedServs/ArchiveInActiveDataList/{from_object}";
            
        }
    }
}
