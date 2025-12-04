using System.Collections.Generic;
using System;

namespace journeyService.Models.hubspot
{
    public class UmiCars
    {
        public string clientid = string.Empty;
        public string firstname = string.Empty;
        public string lastname = string.Empty;
        public string email = string.Empty;
        public string phone = string.Empty;
        public int manufactureyear;
        public string registrationplate = string.Empty;  //encoded code
        public string carmodel = string.Empty;
        public string manufacturer = string.Empty;
        public string lasttreatment_date = string.Empty;
        public string cardeliverydate = string.Empty;
        public int allowdivur;
        public int kodsalesbranch;
        public string salesbranch = string.Empty;
        public int lastgaragecode;
        public string lastgaragename = string.Empty;
        public string associationlable = string.Empty;
        public string dateidkunbaalut = string.Empty;
        public string newregistrationplate = string.Empty; //real car number
        public string contact_association_lable { get; set; } = string.Empty;

        public string date_car_test { get; set; } = string.Empty;//ddmm format 
        public int allow_divur_person;
        public string ops__tafnit_unit = string.Empty;


        public int kodmachzormechira; //field relevant for tafnitavis
        public int kmmechira; //field relevant for tafnitavis
        public int kod_customerclassification; //field relevant for tafnitavis
        public string customerclassification = string.Empty; //field relevant for tafnitavis
        public string agreementdate = string.Empty; //field relevant for tafnitavis


    }
}
