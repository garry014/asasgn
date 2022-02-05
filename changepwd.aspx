<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="changepwd.aspx.cs" Inherits="_200115X_AS.changepwd" %>

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
            grecaptcha.execute('6Lezh_8dAAAAAEJeJ7mM_CwhUg_kx7Un03yZ3Sqk', { action: 'ChangePwd' }).then(function (token) {
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
                <h5 class="text-center mb-4">Change Password</h5>
                <div class="row justify-content-between text-left">
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <label>Current Password:</label>&nbsp;
                    </div>
                    <div class="form-group col-sm-6 flex-column d-flex"> 
                        <asp:TextBox ID="oldpwd" runat="server"></asp:TextBox>
                    </div>
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <!--Intentionally left blank-->
                    </div>
                </div>
                <div class="row justify-content-between text-left">
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <label>New Password:</label>&nbsp;
                    </div>
                    <div class="form-group col-sm-6 flex-column d-flex"> 
                        <asp:TextBox ID="thenewpwd" runat="server" onblur="validate();"></asp:TextBox>
                        <span id="twelvechars">Password Length must be at least 12 characters</span>
                            <span id="lowercase">Password require at least 1 lower case alphabet</span>
                            <span id="uppercase">Password require at least 1 upper case alphabet</span>
                            <span id="numbers">Password require at least 1 number</span>
                            <span id="specialchar">Password require at least 1 special character</span>

                            <script type="text/javascript">
                                function validate() {
                                    var str = document.getElementById('<%=thenewpwd.ClientID %>').value;
                                    document.getElementById('<%=thenewpwd.ClientID %>').style.borderColor = "";

                                    if (str.length < 12) {
                                        document.getElementById("twelvechars").innerHTML = '❌ Password Length must be at least 12 characters';
                                        document.getElementById("twelvechars").style.color = "Red";
                                        document.getElementById('<%=thenewpwd.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("twelvechars").innerHTML = '✔️ Password Length must be at least 12 characters';
                                        document.getElementById("twelvechars").style.color = "Green";
                                    }

                                    if (str.search(/[a-z]/) == -1) {
                                        document.getElementById("lowercase").innerHTML = '❌ Password require at least 1 lower case alphabet';
                                        document.getElementById("lowercase").style.color = "Red";
                                        document.getElementById('<%=thenewpwd.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("lowercase").innerHTML = '✔️ Password require at least 1 lower case alphabet';
                                        document.getElementById("lowercase").style.color = "Green";
                                    }

                                    if (str.search(/[A-Z]/) == -1) {
                                        document.getElementById("uppercase").innerHTML = '❌ Password require at least 1 upper case alphabet';
                                        document.getElementById("uppercase").style.color = "Red";
                                        document.getElementById('<%=thenewpwd.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("uppercase").innerHTML = '✔️ Password require at least 1 upper case alphabet';
                                        document.getElementById("uppercase").style.color = "Green";
                                    }

                                    if (str.search(/[0-9]/) == -1) {
                                        document.getElementById("numbers").innerHTML = '❌ Password require at least 1 number';
                                        document.getElementById("numbers").style.color = "Red";
                                        document.getElementById('<%=thenewpwd.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("numbers").innerHTML = '✔️ Password require at least 1 number';
                                        document.getElementById("numbers").style.color = "Green";
                                    }

                                    if (str.search(/(?=.*[@$!%*#?&])/) == -1) {
                                        document.getElementById("specialchar").innerHTML = '❌ Password require at least 1 special character';
                                        document.getElementById("specialchar").style.color = "Red";
                                    }
                                    else {
                                        document.getElementById("specialchar").innerHTML = '✔️ Password require at least 1 special character';
                                        document.getElementById("specialchar").style.color = "Green";
                                    }
                                }
                            </script>

                    </div>
                    <div class="form-group col-sm-3 flex-column d-flex"> 
			            <!--Intentionally left blank-->
                    </div>
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