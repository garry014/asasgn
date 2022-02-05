using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200115X_AS
{
    public partial class otp : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Email"] == null)
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (checkToken(onetimepass.Text))
            {
                string guid = Guid.NewGuid().ToString();
                Session["AuthToken"] = guid;
                Session["SecurityStamp"] = getStamp(Session["Email"].ToString());
                Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                addAuditLog("login-success", Session["Email"].ToString());
                Response.Redirect("Homepage.aspx", false);
            }
        }

        protected Boolean checkToken(string valtoken)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select UserId,Type,ExpiryTime FROM Authentication WHERE Token=@token AND UserId=@UserId";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@token", valtoken);
            string email = "";
            if (Session["Email"] != null)
            {
                email = Session["Email"].ToString();
            }
            else
            {
                Response.Redirect("/Login.aspx", false);
                return false;
            }
            command.Parameters.AddWithValue("@UserId", email);

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
                                if (reader["Type"].ToString() == "otp")
                                {
                                    invalidateToken(reader["UserId"].ToString(), valtoken);
                                    return true;
                                }

                                
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

        protected void invalidateToken(string userid, string token)
        {
            // Update expiry time to now, to invalidate token
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Authentication SET ExpiryTime=@ExpiryTime WHERE UserId=@UserId AND Token=@Token"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Parameters.AddWithValue("@UserId", userid);
                            cmd.Parameters.AddWithValue("@Token", token);
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
                            //cmd.CommandType = CommandType.Text;
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
    }
}