using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200115X_AS
{
    public partial class Homepage : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
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

                if (DateTime.Now > DateTime.Parse(getMaxAge(Session["Email"].ToString())))
                {
                    Response.Redirect("changepwd.aspx", false);
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
                ClearCookies();
            }
                
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            // apply codes to whenever session timeout
            addAuditLog("logout-success", Session["Email"].ToString());
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("Login.aspx", false);
            ClearCookies();
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

        protected string getMaxAge(string userid)
        {

            string s = null;

            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select MaxAge FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["MaxAge"] != null)
                        {
                            if (reader["MaxAge"] != DBNull.Value)
                            {
                                s = reader["MaxAge"].ToString();
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
    }
}