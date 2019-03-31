using AuctionProjectVer1.Extensions;
using AuctionProjectVer1.Models;
using AuctionProjectVer1.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AuctionProjectVer1.Services
{
    public class AccountService
    {
        public void OpenOrganization(OpenOrganizationViewModel viewModel)
        {
            DataSet applicationDataSet = new DataSet();
            DataSet identityDataSet = new DataSet();
        
            string organizationsTable = "[dbo].[Organizations]";
            string employeeTable = "[dbo].[Employees]";

            Employee employee = new Employee();

            using (TransactionScope transactionScope = new TransactionScope())
            {
                try
                {
                    using (SqlConnection applicationConnection = new SqlConnection(
                                    ApplicationSettings.APPLICATION_CONNECTION_STRING))
                    {
                        applicationConnection.Open();

                        string selectOrganizationByIdentificatorsSql = $"select * from {organizationsTable} " +
                            $"where [IdentificationNumber] = {viewModel.OrganizationIdentificationNumber}";

                        string selectOrganizationsSql = $"select * from {organizationsTable}";

                        using (SqlDataAdapter applicationAdapter = new SqlDataAdapter(selectOrganizationByIdentificatorsSql,
                            applicationConnection))
                        {
                            applicationAdapter.Fill(applicationDataSet, organizationsTable);
                            SqlCommandBuilder applicationCommandBuilder = new SqlCommandBuilder(applicationAdapter);
                            bool isOrganizationAlreadyExist = applicationDataSet.Tables[organizationsTable].Rows.Count != 0;

                            if (isOrganizationAlreadyExist)
                                throw new ApplicationException($"Already has an organization with IdentificationNumber = {viewModel.OrganizationIdentificationNumber}");

                            applicationDataSet.Clear();

                            Organization organization = new Organization()
                            {
                                Id=Guid.NewGuid().ToString(),
                                FullName = viewModel.OrganizationFullName,
                                IdentificationNumber = viewModel.OrganizationIdentificationNumber,
                                OrganizationTypeId = viewModel.OrganizationTypeId,
                                RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            applicationAdapter.SelectCommand = new SqlCommand(selectOrganizationsSql, applicationConnection);
                            applicationCommandBuilder = new SqlCommandBuilder(applicationAdapter);

                            applicationAdapter.Fill(applicationDataSet, organizationsTable);
                            var dataRow = applicationDataSet.Tables[organizationsTable].NewRowWithData(organization);
                            applicationDataSet.Tables[organizationsTable].Rows.Add(dataRow);
                            applicationAdapter.Update(applicationDataSet, organizationsTable);

                            employee.Id = Guid.NewGuid().ToString();
                            employee.FirstName = viewModel.CeoFirstName;
                            employee.LastName = viewModel.CeoLastName;
                            employee.MiddleName = viewModel.CeoMiddleName;
                            employee.Email = viewModel.Email;
                            employee.DoB = viewModel.DoB;
                            employee.OrganizationId = Guid.NewGuid().ToString();
                            

                            string selectEmployeeSql = $"select * from {employeeTable}";

                            applicationAdapter.SelectCommand = new SqlCommand(selectEmployeeSql, applicationConnection);
                            applicationCommandBuilder = new SqlCommandBuilder(applicationAdapter);
                            applicationAdapter.Fill(applicationDataSet, employeeTable);

                            dataRow = applicationDataSet.Tables[employeeTable].NewRowWithData(employee);
                            applicationDataSet.Tables[employeeTable].Rows.Add(dataRow);
                            applicationAdapter.Update(applicationDataSet, employeeTable);

                        }
                    }
                    using (SqlConnection identityConnection = new SqlConnection(
                        ApplicationSettings.IDENTITY_CONNECTION_STRING))
                    {
                        identityConnection.Open();

                        string usersTable = "[dbo].[ApplicationUsers]";
                        string selectUserByEmail = $"select * from {usersTable} where [Email]='{viewModel.Email}'";

                        using (SqlDataAdapter identityUserAdapter = new SqlDataAdapter(selectUserByEmail, identityConnection))
                        {
                            identityUserAdapter.Fill(identityDataSet, usersTable);
                            SqlCommandBuilder identityCommandBuilder = new SqlCommandBuilder(identityUserAdapter);

                            bool isUserAlreadyExist = identityDataSet.Tables[usersTable].Rows.Count != 0;

                            if (isUserAlreadyExist)
                                throw new ApplicationException($"Already has user with email = {viewModel.Email}");

                            identityDataSet.Clear();

                            ApplicationUser user = new ApplicationUser()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Email = viewModel.Email,
                                IsActivatedAccount = true,
                                FailedSigninCount = 0,
                                IsBlockedBySystem = false,
                                AssociatedEmployeeId=employee.Id,
                                CreationDate = DateTime.Now.ToString("yyyy-MM-dd")
                            };
                            string selectUsersSql=$"select * from {usersTable}";
                            identityUserAdapter.SelectCommand = new SqlCommand(selectUsersSql, identityConnection);
                            identityCommandBuilder = new SqlCommandBuilder(identityUserAdapter);
                            identityUserAdapter.Fill(identityDataSet, usersTable);
                            var dataRow = identityDataSet.Tables[usersTable].NewRowWithData(user);
                            identityDataSet.Tables[usersTable].Rows.Add(dataRow);
                            identityUserAdapter.Update(identityDataSet,usersTable);

                            identityDataSet.Clear();

                            ApplicationUserPasswordHistories userPassword = new ApplicationUserPasswordHistories()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ApplicationUserId = user.Id,
                                SetupDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                InvalidatedDate = DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"),
                                PasswordHash = viewModel.Password
                            };

                            string usersPasswordsTable = "[dbo].[ApplicationUserPasswordHistories]";
                            string SelectUserPasswordSql = $"select * from {usersPasswordsTable}";
                            identityUserAdapter.SelectCommand = new SqlCommand(SelectUserPasswordSql, identityConnection);
                            identityCommandBuilder = new SqlCommandBuilder(identityUserAdapter);

                            identityUserAdapter.Fill(identityDataSet, usersPasswordsTable);
                            dataRow = identityDataSet.Tables[usersPasswordsTable].NewRowWithData(userPassword);
                            identityDataSet.Tables[usersPasswordsTable].Rows.Add(dataRow);
                            identityUserAdapter.Update(identityDataSet, usersPasswordsTable);

                            identityDataSet.Clear();

                            GeoLocationInfo geoLocationInfo = GetGeolocationInfo();

                            ApplicationUserSignInHistories userSignIn = new ApplicationUserSignInHistories()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ApplicationUserId = user.Id,
                                SignInTime = DateTime.Now.ToString("yyyy-MM-dd"),
                                MachineIp=geoLocationInfo.ip,
                                IpToGeoCountryCode=geoLocationInfo.country_name,
                                IpToGeoCityName=geoLocationInfo.city,
                                IpToGeoLatitude=geoLocationInfo.latitude,
                                IpToGeoLongitude=geoLocationInfo.longitude
                            };

                            string userSignInTable= "[dbo].[ApplicationUserSignInHistories]";
                            string userSignInSql = $"select * from {userSignInTable}";
                            identityUserAdapter.SelectCommand = new SqlCommand(userSignInSql, identityConnection);
                            identityCommandBuilder = new SqlCommandBuilder(identityUserAdapter);

                            identityUserAdapter.Fill(identityDataSet, userSignInTable);
                            dataRow = identityDataSet.Tables[userSignInTable].NewRowWithData(userSignIn);
                            identityDataSet.Tables[userSignInTable].Rows.Add(dataRow);
                            identityUserAdapter.Update(identityDataSet, userSignInTable);

                        }
                      
                    }

                    transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public GeoLocationInfo GetGeolocationInfo()
        {
            WebClient webClient = new WebClient();
            string externalIp = webClient
                .DownloadString("http://icanhazip.com");

            string ipStackAccessKey = "1919678931125515ad3f7b04e39ce87e";
            string ipStackUrl = $"api.ipstack.com/{externalIp}?access_key={ipStackAccessKey}";
            ipStackUrl = "http://" + ipStackUrl;

            string ipInfoAsJson = webClient.DownloadString(ipStackUrl);
            GeoLocationInfo geolocationInfo = JsonConvert.DeserializeObject<GeoLocationInfo>(ipInfoAsJson);
            return geolocationInfo;
        }

        public void ChangeUserPassword(ChangePasswordViewModel viewModel)
        {
            DataSet identityDataSet = new DataSet();

            string usersTable = "[dbo].[ApplicationUsers]";
            string userPasswordHistoriesTable = "[dbo].[ApplicationUserPasswordHistories]";
            string userSignInHistoriesTable = "[dbo].[ApplicationUserSignInHistories]";

            using (TransactionScope transactionScope = new TransactionScope())
            {
                try
                {
                    using (SqlConnection identityConnection=new SqlConnection(ApplicationSettings.IDENTITY_CONNECTION_STRING))
                    {
                        identityConnection.Open();
                        string selectPasswordByEmail = $"select u.Email, u.Id, p.SetupDate, p.InvalidatedDate, p.PasswordHash" +
                            $" from {usersTable} u, {userPasswordHistoriesTable} p where u.Id=p.ApplicationUserId" +
                            $" and u.Email='{viewModel.Email}' order by p.SetupDate desc";

                        using (SqlDataAdapter identityAdapter=new SqlDataAdapter(selectPasswordByEmail,identityConnection))
                        {
                            identityAdapter.Fill(identityDataSet);
                            SqlCommandBuilder identityCommandBuilder = new SqlCommandBuilder(identityAdapter);
       
                            var tablePasswordsSelected = identityDataSet.Tables[0];
                            var rows = tablePasswordsSelected.Rows;
                            var rowsNum = tablePasswordsSelected.Rows.Count;
                            int rowsCount = 0;
                            if (rowsNum < 5)
                            {
                                rowsCount = rowsNum;
                            }
                            else
                            {
                                rowsCount = 5;
                            }
                            for (int i = 0; i < rowsCount; i++)
                            {
                                if (rows[i][4].ToString() == viewModel.newPassword)
                                {
                                    throw new ApplicationException($"{viewModel.newPassword} was before. Choose another password");
                                }
                            }
                            var userId = tablePasswordsSelected.Rows[0]["Id"];

                            ApplicationUserPasswordHistories PasswordHistories = new ApplicationUserPasswordHistories()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ApplicationUserId = userId.ToString(),
                                SetupDate=DateTime.Now.ToString("yyyy-MM-dd"),
                                InvalidatedDate= DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"),
                                PasswordHash=viewModel.newPassword
                            };
                            string userPasswordSql = $"select * from {userPasswordHistoriesTable}";
                            identityAdapter.SelectCommand = new SqlCommand(userPasswordSql, identityConnection);
                            identityCommandBuilder = new SqlCommandBuilder(identityAdapter);

                            identityAdapter.Fill(identityDataSet, userPasswordHistoriesTable);
                            var dataRow = identityDataSet.Tables[userPasswordHistoriesTable].NewRowWithData(PasswordHistories);
                            identityDataSet.Tables[userPasswordHistoriesTable].Rows.Add(dataRow);
                            identityAdapter.Update(identityDataSet, userPasswordHistoriesTable);

                        }

                    }
                    transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
                
        }
    }
}
