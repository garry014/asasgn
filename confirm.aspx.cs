using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200115X_AS
{
    public partial class confirm : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            lbl_message.Text = "Verfication link does not exists, or has expired. Please retry, thankiew.";

            if (Request.QueryString["key"] is object)
            {
                if (getToken(Request.QueryString["key"].ToString()))
                    lbl_message.Text = "Successfully confirmed";
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
            
        }


        protected Boolean getToken(string valtoken)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select UserId,Type,ExpiryTime FROM Authentication WHERE Token=@token";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@token", valtoken);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["UserId"] != null)
                        {
                            System.Diagnostics.Debug.WriteLine(DateTime.Now);
                            System.Diagnostics.Debug.WriteLine(reader["ExpiryTime"]);
                            var expiry = DateTime.Parse(reader["ExpiryTime"].ToString());
                            if (expiry >= DateTime.Now)
                            {
                                System.Diagnostics.Debug.WriteLine("Should be valid");
                                if (reader["Type"].ToString() == "reg")
                                {
                                    updateEmailConf(reader["UserId"].ToString());
                                    invalidateToken(reader["UserId"].ToString());
                                }
                                    
                                return true;
                            }
                            return false;
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
            return false;

        }

        protected void updateEmailConf(string userid)
        {
            // Update confirm email
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET IsEmailVerified=@IsEmailVerified WHERE Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Parameters.AddWithValue("@USERID", userid);
                            cmd.Parameters.AddWithValue("@IsEmailVerified", 1);

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

        protected void invalidateToken(string userid)
        {
            // Update expiry time to now, to invalidate token
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Authentication SET ExpiryTime=@ExpiryTime WHERE UserId=@UserId"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Parameters.AddWithValue("@UserId", userid);
                            cmd.Parameters.AddWithValue("@ExpiryTime", DateTime.Now);          

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
    }
}