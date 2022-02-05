using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Configuration;

namespace _200115X_AS
{
    public partial class Registration : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string cvvHash;
        static string salt;
        static string securitystamp;
        HttpContext ctx;
        byte[] Key;
        byte[] IV;
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".PNG" };

        protected void Page_Load(object sender, EventArgs e)
        {
            //sendVerificationEmail();
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            submit.Enabled = false;
            string errors = "";
            if (ValidateCaptcha())
            {
                if (!checkPassword(password.Text))
                    errors = errors + "Weak Password, please follow the password requirements.<br>";
                if (findEmailExists(email.Text.Trim()))
                    errors = errors + "Email is already registered.<br>";
                if (!validEmail(email.Text))
                    errors = errors + "Please use a valid email address.<br>";
                if (!validName(fname.Text))
                    errors = errors + "Invalid first name.<br>";
                if (!validName(lname.Text))
                    errors = errors + "Invalid last name.<br>";
                if (!validExpiry(expiry.Text))
                    errors = errors + "Invalid credit card expiry <br>";
                if (!validCard(ccard.Text))
                    errors = errors + "Invalid credit card numbers <br>";
                if (!validCVC(cvv.Text))
                    errors = errors + "Invalid CVV <br>";
                errors = errors + validDOB(dob.Text);
                // Image validation
                if (!imgUpload.HasFile)
                {
                    errors = errors + "A valid image should be uploaded.<br>";
                }
                else if (Path.GetExtension(imgUpload.FileName) != ".jpg")
                {
                    errors = errors + "Only jpg file type is accepted.<br>";
                }
                else if (imgUpload.PostedFile.ContentLength > (200 * 1024))
                {
                    // file size < 0.2mb
                    errors = errors + "File size should be less than 0.5mb<br>";
                }


                if (string.IsNullOrEmpty(errors))
                {
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    byte[] saltByte = new byte[8];
                    byte[] stampByte = new byte[16];

                    rng.GetBytes(saltByte);
                    rng.GetBytes(stampByte);
                    salt = Convert.ToBase64String(saltByte);
                    securitystamp = Convert.ToBase64String(stampByte);
                    string pwdWithSalt = password.Text.ToString().Trim() + salt;

                    SHA512Managed hashing = new SHA512Managed();
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                    finalHash = Convert.ToBase64String(hashWithSalt);

                    string cvvWithSalt = cvv.Text.ToString().Trim() + salt;
                    byte[] cvvSalted = hashing.ComputeHash(Encoding.UTF8.GetBytes(cvvWithSalt));
                    cvvHash = Convert.ToBase64String(cvvSalted);



                    RijndaelManaged cipher = new RijndaelManaged();
                    cipher.GenerateKey();
                    Key = cipher.Key;
                    IV = cipher.IV;


                    createAccount();

                    sendVerificationEmail(email.Text.Trim(), generateSecureToken(email.Text.Trim()));
                    Response.Redirect("Login.aspx", false);
                }

                lbl_pwdchecker.Text = "Error(s) : " + errors;
                lbl_pwdchecker.ForeColor = Color.Red;
                submit.Enabled = true;
                return;
            }
            else
            {
                errors = errors + "Robots shall not pass!<br>";
                lbl_pwdchecker.Text = "Error(s) : " + errors;
                lbl_pwdchecker.ForeColor = Color.Red;
                return;
            }
            
        }

      

        protected void createAccount()
        {
            
            try
            {
                FileUpload img = imgUpload;
                Byte[] imgByte = null;
                if (img.HasFile && img.PostedFile != null)
                {
                    //To create a PostedFile
                    HttpPostedFile File = imgUpload.PostedFile;
                    //Create byte Array with file len
                    imgByte = new Byte[File.ContentLength];
                    //force the control to load data in array
                    File.InputStream.Read(imgByte, 0, File.ContentLength);
                }
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@Fname, @Lname, @DOB, @Photo, @Email, @PasswordHash, @PasswordSalt, @IV, @Key, @CardNumber, @ExpiryDate, @CVV, @DateTimeRegistered, @IsEmailVerified, @LoginFailAttempts, @AccRecovery, @SecurityStamp, @MinAge, @MaxAge, @SecondPw, @FirstPw)"))
                {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Fname", fname.Text);
                            cmd.Parameters.AddWithValue("@Lname", lname.Text);
                            cmd.Parameters.AddWithValue("@DOB", dob.Text);
                            cmd.Parameters.AddWithValue("@Photo", imgByte); //sample text
                            cmd.Parameters.AddWithValue("@Email", email.Text.Trim());
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@DateTimeRegistered", DateTime.Now);
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@CardNumber", Convert.ToBase64String(encryptData(ccard.Text.Trim())));
                            cmd.Parameters.AddWithValue("@ExpiryDate", Convert.ToBase64String(encryptData(expiry.Text.Trim())));
                            cmd.Parameters.AddWithValue("@CVV", cvvHash);
                            cmd.Parameters.AddWithValue("@IsEmailVerified", 0);
                            cmd.Parameters.AddWithValue("@LoginFailAttempts", 0);
                            DateTime? accRecovery = null;
                            cmd.Parameters.Add("@AccRecovery", accRecovery).Value = DBNull.Value;
                            cmd.Parameters.AddWithValue("@SecurityStamp", securitystamp);
                            cmd.Parameters.AddWithValue("@MinAge", DateTime.Now.AddMinutes(1));
                            cmd.Parameters.AddWithValue("@MaxAge", DateTime.Now.AddMinutes(5));
                            string SecondPw = null;
                            string FirstPw = null;
                            cmd.Parameters.Add("@SecondPw", SecondPw).Value = DBNull.Value;
                            cmd.Parameters.Add("@FirstPw", FirstPw).Value = DBNull.Value;

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

        protected void Calendar1_DayRender(object sender, DayRenderEventArgs e)
        {

            if (e.Day.Date > DateTime.Today)
            {

                e.Day.IsSelectable = false;
            }

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

        private Boolean validEmail(string email)
        {
            Regex regex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"+ "@"+ @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            return false;
        }

        protected Boolean findEmailExists(string email)
        {
            Boolean emailExists = false;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Email FROM Account WHERE Email=@EMAIL";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@EMAIL", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["Email"] != null)
                        {
                            emailExists = true;
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
            return emailExists;
        }

        private Boolean validName(string name)
        {
            Regex regex = new Regex(@"^[a-zA-Z.+'-]+(?:\s[a-zA-Z.+'-]+)*\s?$");
            Match match = regex.Match(name);
            if (match.Success)
                return true;
            return false;
        }

        private Boolean validExpiry(string expiry)
        {
            var mm = Int32.Parse(DateTime.Now.ToString("MM"));
            var yy = Int32.Parse(DateTime.Now.ToString("yy"));
            Regex regex = new Regex(@"/^\d{2}$/");

            try
            {
                string[] expiryList = expiry.Split('/');
                if (Int32.Parse(expiryList[0]) < 1 || Int32.Parse(expiryList[0]) > 12)
                    return false;

                if (string.IsNullOrEmpty(expiryList[1]))
                    return false;

                if (expiryList.Length > 2)
                    return false;

                if (Int32.Parse(expiryList[1]) < yy)
                    return false;

                if (Int32.Parse(expiryList[0]) <= mm && Int32.Parse(expiryList[1]) == yy)
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }
            
            return true;
        }

        private Boolean validCard(string ccard)
        {
            Regex regex = new Regex(@"^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$");
            Match match = regex.Match(ccard);
            if (match.Success)
                return true;
            return false;
        }

        private Boolean validCVC(string ccard)
        {
            Regex regex = new Regex(@"^[0-9]{3}$");
            Match match = regex.Match(ccard);
            if (match.Success)
                return true;
            return false;
        }

        private string validDOB(string dob)
        {
            string errors = "";
            DateTime bday;
            try
            {
                bday = DateTime.Parse(dob);
            }
            catch
            {
                errors = "Incorrect date format.<br>";
                return errors;
            }
            
            DateTime today = DateTime.Today;
            int age = today.Year - bday.Year;
            if (age < 12)
                errors = errors + "You should be at least 12 year old to register. <br>";
            if (age > 70) 
                errors = errors + "You need to be below 70 year old to register. <br>";
            return errors;
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);


                //Encrypt
                //cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
                //cipherString = Convert.ToBase64String(cipherText);
                //Console.WriteLine("Encrypted Text: " + cipherString);

            }
            catch (Exception ex)
            {
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            finally { }
            return cipherText;
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

        private void sendVerificationEmail(string recipientEmail, string tokenID)
        {
            
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("SITConnect", ConfigurationManager.AppSettings["EmailAddress"].ToString()));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Confirm your account to continue - SITConnect";
            message.Body = new TextPart("html")
            {
                Text = @"<a href='https://localhost:44377/confirm.aspx?key=" + tokenID + "'>Click this link to finish registering.</a> This link will expire within 15 minutes."
            };

            System.Diagnostics.Debug.WriteLine("https://localhost:44377/confirm.aspx?key=" + tokenID);
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(ConfigurationManager.AppSettings["EmailAddress"].ToString(), ConfigurationManager.AppSettings["EmailPassword"].ToString());
                client.Send(message);

            }
            catch (Exception ex)
            {
                // Show some error msg????
                Response.Redirect("/CustomError/GenricError.html", false);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        private string generateSecureToken(string userid)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] tokenByte = new byte[32];
            rng.GetBytes(tokenByte);
            string token = Convert.ToBase64String(tokenByte);
            token = new string((from c in token
                              where char.IsLetterOrDigit(c)
                              select c
            ).ToArray());

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
                            cmd.Parameters.AddWithValue("@UserId", userid);
                            cmd.Parameters.AddWithValue("@Token", token);
                            cmd.Parameters.AddWithValue("@Type", "reg");
                            cmd.Parameters.AddWithValue("@Timestamp", timeNow);
                            cmd.Parameters.AddWithValue("@ExpiryTime", timeNow.AddMinutes(15));

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

            return token;
        }
    }
}