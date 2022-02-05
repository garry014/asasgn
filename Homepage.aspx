<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="_200115X_AS.Homepage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Home</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Quicksand&display=swap" rel="stylesheet"/>
    <link href="/StyleSheet1.css" rel="stylesheet"/>

    <script>
        //session end 
        var sTimeout = (1 * 60 * 1000) + 1;
        setTimeout('SessionEnd()', sTimeout);

        function SessionEnd() {
            window.location.reload();
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h3>Welcome to SITConnect</h3>
        <asp:Button ID="Logout" class="btn-block btn-primary" runat="server" Text="Logout" OnClick="Logout_Click" />
        <a href="/changepwd.aspx">Change Password?</a>
    </form>
</body>
</html>
