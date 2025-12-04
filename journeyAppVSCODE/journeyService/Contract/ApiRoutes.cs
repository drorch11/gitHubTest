namespace journeyService.Contract
{
    public static class ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = $"{Root}/{Version}";

        public static class Posts
        {
            public const string GetAll = Base + "/posts";
            public const string Get = Base + "/posts/{postid}";
            public const string Create = Base + "/posts";
            public const string Update = Base + "/posts/{postid}";
            public const string Delete = Base + "/posts/{postid}";

        }

        public static class flycardServ
        {
            public const string GetAll = Base + "/flycardServs";
            public const string Get = Base + "/flycardServs/{postid}";
            public const string Create = Base + "/flycardServs";
            public const string Update = Base + "/flycardServs/{postid}";
            public const string Delete = Base + "/flycardServs/{postid}";
            
            public const string GetpopulationForReminder = Base + "/flycardServs/populationForReminder/{codeAction}/{maxDays}/{LogIdInforU}";
            public const string GetpopulationForPaymentReminder = Base + "/flycardServs/populationForPaymentReminder/{codeAction}/{maxDays}/{LogIdInforU}";
        }
        public static class hubspotUrlServ
        {
            public const string Get = Base + "/hubspotUrlServs/{pagename}/{seccode}";

        }
        public static class InforUServ
        {
            public const string GetUnsubscribeList = Base + "/InforUServ/GetUnsubscribeList/{codeaction}/{propertyType}";

        }

        public static class tafnitServ
        {
            public const string RemoveDivurByPhoneOrEmail = Base + "/tafnitServ/RemoveDivurByPhoneOrEmail/{codeaction}/{propertyType}/{propertyValue}/{logID_HubSpotImportLog}/{tafnitUnit}";
            
        }
        public static class hubspotServ
        {
            //public const string removeContactFunc = Base + "/hubspotServ/removeContact/{codeaction}/{email}";
            //public const string updateanddeleteAssociatedLabelFunc = Base + "/hubspotServ/updateanddeleteAssociatedLabel/{codeaction}";

        }


    }
}
