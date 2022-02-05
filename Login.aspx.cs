using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MailKit.Net.Smtp;
using MimeKit;

namespace _200115X_AS
{
    public partial class Login : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string errorMsg = "The login email or password given is not valid. Please try again.";
        static int loginAttemptsFailed = 0;
        static DateTime? accRecovery = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(loginemail.Text) || string.IsNullOrEmpty(password.Text))
            {
                lbl_error.Text = "Please fill up both login email and password.";
                return;
            }

            if (ValidateCaptcha())
            {
                string email = loginemail.Text.ToString().Trim();
                string pwd = password.Text.ToString().Trim();

                SHA512Managed hashing = new SHA512Managed();
                string dbHash = getDBHash(email);
                string dbSalt = getDBSalt(email);
                bool isEmailVerified = Convert.ToBoolean(getEmailVerified(email));
                string securitystamp = getStamp(email);
                if (isEmailVerified != true)
                {
                    lbl_error.Text = "Please confirm your email before logging in.";
                    return;
                }

                try
                {
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string pwdWithSalt = pwd + dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);

                        if (loginAttemptsFailed >= 3)
                        {
                            
                            System.Diagnostics.Debug.WriteLine(accRecovery.HasValue);
                            System.Diagnostics.Debug.WriteLine(accRecovery <= DateTime.Now);
                            if (accRecovery.HasValue && accRecovery <= DateTime.Now)
                            {
                                loginAttemptsFailed = -1;
                                accRecovery = null;
                                System.Diagnostics.Debug.WriteLine("Passing null to accrecovery nw");
                                AddFailCount(email);
                                if (userHash.Equals(dbHash))
                                {
                                    sendVerificationEmail(email);
                                    Session["Email"] = email;

                                    
                                    Response.Redirect("otp.aspx", false);
                                }
                                else
                                {
                                    lbl_error.Text = errorMsg;
                                    AddFailCount(email);
                                    addAuditLog("login-failed", email);
                                    return;
                                }
                            }
                            lbl_error.Text = "The account has been locked, please wait for 1 min to unlock.";
                            return;
                        }
                        else if (userHash.Equals(dbHash))
                        {
                            if (loginAttemptsFailed > 0)
                            {
                                loginAttemptsFailed = -1;
                                AddFailCount(email);
                            }
                            sendVerificationEmail(email);
                            Session["Email"] = email;

                            
                            Response.Redirect("otp.aspx", false);
                        }
                        else
                        {
                            lbl_error.Text = errorMsg;
                            AddFailCount(email);
                            addAuditLog("login-failed", email);
                            return;
                        }
                    }
                    lbl_error.Text = errorMsg;
                    return;
                }
                catch (Exception ex)
                {
                    Response.Redirect("/CustomError/GenricError.html", false);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                finally { }
            }
            lbl_error.Text = "You are known as a bot, and you shall not pass!";
            return;
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
        protected string getEmailVerified(string userid)
        {

            string s = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select IsEmailVerified FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["IsEmailVerified"] != null)
                        {
                            if (reader["IsEmailVerified"] != DBNull.Value)
                            {
                                s = reader["IsEmailVerified"].ToString();
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

        protected string getDBHash(string userid)
        {

            string h = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash, LoginFailAttempts, AccRecovery FROM Account WHERE Email=@USERID";
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
                                loginAttemptsFailed = Int32.Parse(reader["LoginFailAttempts"].ToString());
                                System.Diagnostics.Debug.WriteLine(loginAttemptsFailed);
                                if(!String.IsNullOrEmpty(reader["AccRecovery"].ToString()))
                                {
                                    accRecovery = DateTime.Parse(reader["AccRecovery"].ToString());
                                }
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

        protected void AddFailCount(string userid)
        {
            // Update account attempt try
            try
            {
                loginAttemptsFailed = loginAttemptsFailed + 1;
                if(loginAttemptsFailed == 3)
                {
                    if (!accRecovery.HasValue)
                    {
                        accRecovery = DateTime.Now.AddMinutes(1);
                        System.Diagnostics.Debug.WriteLine("Adding expiry");
                    }
                    
                   
                    //else if (accRecovery <= DateTime.Now)
                    //{

                    //}
                }

                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Is Null? " + !accRecovery.HasValue);
                    
                        using (SqlCommand cmd = new SqlCommand("UPDATE Account SET LoginFailAttempts=@LoginFailAttempts, AccRecovery=@AccRecovery WHERE Email=@USERID"))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                if (accRecovery.HasValue)
                                {
                                    cmd.Parameters.AddWithValue("@USERID", userid);
                                    cmd.Parameters.AddWithValue("@LoginFailAttempts", loginAttemptsFailed);
                                    cmd.Parameters.AddWithValue("@AccRecovery", accRecovery);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@USERID", userid);
                                    cmd.Parameters.AddWithValue("@LoginFailAttempts", loginAttemptsFailed);
                                    cmd.Parameters.Add("@AccRecovery", accRecovery).Value = DBNull.Value;
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


        protected void addAuditLog(string action, string userid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO AuditLogs VALUES(@UserId, @Action, @Timestamp)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@UserId", userid);
                            cmd.Parameters.AddWithValue("@Action", action);
                            cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            
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

        public class MyObject 
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        // OTP
        private void sendVerificationEmail(string recipientEmail)
        {
            Random rnd = new Random();
            string tokenID = (rnd.Next(100000, 999999)).ToString();
            var timeNow = DateTime.Now;

            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Authentication VALUES(@UserId, @Token, @Type, @Timestamp, @ExpiryTime)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@UserId", recipientEmail);
                            cmd.Parameters.AddWithValue("@Token", tokenID);
                            cmd.Parameters.AddWithValue("@Type", "otp");
                            cmd.Parameters.AddWithValue("@Timestamp", timeNow);
                            cmd.Parameters.AddWithValue("@ExpiryTime", timeNow.AddMinutes(5));

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

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("SITConnect", "peterleo590@gmail.com"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Proceed to Login - SITConnect";
            message.Body = new TextPart("html")
            {
                Text = @"Your One-Time Password is " + tokenID + ". This OTP will expire within 5 minutes."
            };

            System.Diagnostics.Debug.WriteLine("OTP generated" + tokenID);
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(ConfigurationManager.AppSettings["EmailAddress"].ToString(), ConfigurationManager.AppSettings["EmailPassword"].ToString());
                client.Send(message);

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

    }

    
}