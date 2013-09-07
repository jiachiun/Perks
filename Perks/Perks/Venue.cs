using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perks
{
    public class Venue
    {
        public int index;
        public string id;
        public string name;
        public string imageType;
        public string imageVenue;
        public string canonicalUrl;

        // location
        public string address;
        public string longitude;
        public string latitude;
        public string distance;

        // specials
        public string message;
        public string description;
        public string fineprint = "";

        // stats
        public string checkinsCount;
        public string usersCount;


        public Venue()
        {

        }


    }
}
