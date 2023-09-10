using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocSumarizer
{
    public class AppConstants
    {
         public static string OpenAIApiKey = "";//"-- Open AI Key --";
        public static string OrgID = "";//"-- ORG ID --";
        public static string Model = "";
     
        public static (string model, string apiKey, string orgId) GetSettings()
        {
            
            return (Model,OpenAIApiKey, OrgID);
        }
       public static void LoadFromSettings()
        {
            OpenAIApiKey = ConfigurationManager.AppSettings["OpenAIApiKey"];
            OrgID = ConfigurationManager.AppSettings["OrgID"];
            Model = ConfigurationManager.AppSettings["Model"];
        }
    }

    
   
}
