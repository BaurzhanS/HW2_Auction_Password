using AuctionProjectVer1.Services;
using AuctionProjectVer1.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuctionProjectVer1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AccountService s = new AccountService();
            //s.OpenOrganization(new OpenOrganizationViewModel()
            //{
            //    OrganizationFullName = "Air Astana",
            //        OrganizationIdentificationNumber="159753466",
            //        OrganizationTypeId= "CBA1939F-860E-417E-9B7C-0C008BB7AD93",
            //        CeoFirstName="Бауыржан",
            //        CeoLastName="Султанкулов",
            //        CeoMiddleName="Русланович",
            //        Email="abcd@mail.ru",
            //        DoB=new DateTime(1985,7,16).ToShortDateString(),
            //        Password="12345",
            //        PasswordConfirmation= "12345"
            //});
            //s.GetGeolocationInfo();
            s.ChangeUserPassword(new ChangePasswordViewModel()
            {
                Email="ddd@mail.ru",
                oldPassword= "qazwsx",
                newPassword= "qwerty",
                newPasswordConfirmation= "qwerty"
            });

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
