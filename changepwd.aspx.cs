using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static _200115X_AS.Login;

namespace _200115X_AS
{
    public partial class changepwd : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        static string securitystamp;
        static string dbHash;
        static string FirstPw;
        static string SecondPw;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Email"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null && Session["SecurityStamp"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                    ClearCookies();
                }

                if (!getStamp(Session["Email"].ToString()).Equals(Session["SecurityStamp"]))
                {
                    Response.Redirect("Login.aspx", false);
                    ClearCookies();
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
                ClearCookies();
            }
        }

        private void ClearCookies()
        {
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                if (Session["Email"] == null)
                {
                    lbl_error.Text = "Session timeout, please login again to continue.";
                    return;
                }
                if (!checkPassword(thenewpwd.Text))
                {
                    lbl_error.Text = "Weak Password, please follow the password requirements.<br>";
                    return;
                }
                if (DateTime.Now < DateTime.Parse(getMinAge(Session["Email"].ToString())))
                {
                    lbl_error.Text = "Don't kiasu plz, can only change your password 1 min later muhahaha.<br>";
                    return;
                }

                string email = Session["Email"].ToString();
                string pwd = oldpwd.Text.ToString().Trim();
                string newpwd = thenewpwd.Text.ToString().Trim();

                SHA512Managed hashing = new SHA512Managed();
                dbHash = getDBHash(email);
                string dbSalt = getDBSalt(email);

                
                try
                {
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string pwdWithSalt = pwd + dbSalt;
                        salt = dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);

                        if (userHash.Equals(dbHash))
                        {
                            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                            byte[] stampByte = new byte[16];
                            rng.GetBytes(stampByte);
                            securitystamp = Convert.ToBase64String(stampByte);

                            string newpwdWithSalt = thenewpwd.Text.ToString().Trim() + salt;

                            byte[] newhashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(newpwdWithSalt));
                            finalHash = Convert.ToBase64String(newhashWithSalt);

                            if (finalHash == dbHash || finalHash == FirstPw || finalHash == SecondPw)
                            {
                                lbl_error.Text = "Your password cannot be the same as the past 2 passwords, and your current one.";
                                return;
                            }
                            changepassword(email);
                            Response.Redirect("Homepage.aspx", false);
                        }
                        
                    }
                    lbl_error.Text = "Your current password is incorrect.";
                    return;
                }
                catch (Exception ex)
                {
                    Response.Redirect("/CustomError/GenricError.html", false);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                finally { }
            }
        }

        protected string getStamp(string userid)
        {

            string s = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select SecurityStamp FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["SecurityStamp"] != null)
                        {
                            if (reader["SecurityStamp"] != DBNull.Value)
                            {
                                s = reader["SecurityStamp"].ToString();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            finally { connection.Close(); }
            return s;

        }

        protected string getMinAge(string userid)
        {

            string s = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select MinAge, FirstPw, SecondPw FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["MinAge"] != null)
                        {
                            if (reader["MinAge"] != DBNull.Value)
                            {
                                s = reader["MinAge"].ToString();
                                FirstPw = reader["FirstPw"].ToString();
                                SecondPw = reader["SecondPw"].ToString();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            finally { connection.Close(); }
            return s;

        }

        protected void changepassword(string userid)
        {
            // Update account attempt try
            try
            {
                

                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PasswordHash=@PasswordHash, SecurityStamp=@SecurityStamp, MinAge=@MinAge, MaxAge=@MaxAge, SecondPw=@SecondPw, FirstPw=@FirstPw WHERE Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            
                            cmd.Parameters.AddWithValue("@USERID", userid);
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@SecurityStamp", securitystamp);
                            cmd.Parameters.AddWithValue("@MinAge", DateTime.Now.AddMinutes(1));
                            cmd.Parameters.AddWithValue("@MaxAge", DateTime.Now.AddMinutes(5));

                            if (string.IsNullOrEmpty(FirstPw))
                            {
                                cmd.Parameters.Add("@SecondPw", FirstPw).Value = DBNull.Value;
                                cmd.Parameters.AddWithValue("@FirstPw", dbHash);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@SecondPw", FirstPw);
                                cmd.Parameters.AddWithValue("@FirstPw", dbHash);
                            }

                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        protected string getDBHash(string userid)
        {

            string h = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM Account WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            finally { connection.Close(); }


            return h;
        }

        protected string getDBSalt(string userid)
        {

            string s = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PASSWORDSALT FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PASSWORDSALT"] != null)
                        {
                            if (reader["PASSWORDSALT"] != DBNull.Value)
                            {
                                s = reader["PASSWORDSALT"].ToString();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            finally { connection.Close(); }
            return s;

        }

        private Boolean checkPassword(string password)
        {
            if (password.Length < 12)
            {
                return false;
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, "(?=.*[@$!%*#?&])"))
            {
                return false;
            }

            return true;
        }
        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6Lezh_8dAAAAAChJCFlctyjFVHk65-I_uAaV_DBu&response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }

                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }
}