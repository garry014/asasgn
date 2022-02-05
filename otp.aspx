<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="otp.aspx.cs" Inherits="_200115X_AS.otp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Quicksand&display=swap" rel="stylesheet"/>
    <link href="/StyleSheet1.css" rel="stylesheet"/>
    <script src="https://www.google.com/recaptcha/api.js?render=6Lezh_8dAAAAAEJeJ7mM_CwhUg_kx7Un03yZ3Sqk"></script>
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6Lezh_8dAAAAAEJeJ7mM_CwhUg_kx7Un03yZ3Sqk', { action: 'otp' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>

</head>
<body>
    <form id="form1" runat="server">
    <div class="container-fluid px-1 py-5 mx-auto">
    <div class="row d-flex justify-content-center">
        <div class="col-xl-7 col-lg-8 col-md-9 col-11 text-center">
            <div class="card">
                <h5 class="text-center mb-4">Login</h5>
                <div class="row justify-content-between text-left">
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <label>OTP:</label>&nbsp;
                    </div>
                    <div class="form-group col-sm-6 flex-column d-flex"> 
                        <asp:TextBox ID="onetimepass" runat="server"></asp:TextBox>
                    </div>
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <!--Intentionally left blank-->
                    </div>
                </div>
                <div class="row justify-content-between text-left">
                    <div class="row justify-content-end">
                        <div class="form-group col-sm-12">
                            <asp:Label ID="lbl_error" runat="server" CssClass="text-danger"></asp:Label><br />
                            <asp:Button ID="submit" class="btn-block btn-primary" runat="server" Text="Submit" OnClick="submit_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
        <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
    </form>

    
</body>
</html>