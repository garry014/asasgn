<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="_200115X_AS.Registration" %>

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
            grecaptcha.execute('6Lezh_8dAAAAAEJeJ7mM_CwhUg_kx7Un03yZ3Sqk', { action: 'Register' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>

</head>
<body>
    <div class="container-fluid px-1 py-5 mx-auto">
    <div class="row d-flex justify-content-center">
        <div class="col-xl-7 col-lg-8 col-md-9 col-11 text-center">
            <div class="card">
                <h5 class="text-center mb-4">Registration</h5>
                <form id="form1" runat="server">
                    <div class="row justify-content-between text-left">
                        <div class="form-group col-sm-12 flex-column d-flex"> <label class="form-control-label px-1">Email Address<span class="text-danger"> *</span></label>&nbsp;
                            <asp:TextBox ID="email" onblur="checkEmail();" runat="server"></asp:TextBox>
                            <span id="emailError" class="text-danger" style="display:none;">Please enter a valid email address</span>
                            
                            <script>
                                function checkEmail() {
                                    var str = document.getElementById('<%=email.ClientID %>').value;
                                    var pattern = /^\w+[\+\.\w-]*@([\w-]+\.)*\w+[\w-]*\.([a-z]{2,4}|\d+)$/i;

                                    document.getElementById('emailError').style.display = "none";
                                    document.getElementById('<%=email.ClientID %>').style.borderColor = "";

                                    if (!pattern.test(str)) {
                                        document.getElementById('emailError').style.display = "block";
                                        document.getElementById('<%=email.ClientID %>').style.borderColor = "red";
                                    }
                                }
                            </script>
                        </div>
                    </div>
                    <div class="row justify-content-between text-left">
                        <div class="form-group col-sm-12 flex-column d-flex"> <label class="form-control-label px-1">Password<span class="text-danger"> *</span></label>&nbsp;
                            <asp:TextBox ID="password" runat="server" onblur="validate();"></asp:TextBox>
                            <span id="twelvechars">Password Length must be at least 12 characters</span>
                            <span id="lowercase">Password require at least 1 lower case alphabet</span>
                            <span id="uppercase">Password require at least 1 upper case alphabet</span>
                            <span id="numbers">Password require at least 1 number</span>
                            <span id="specialchar">Password require at least 1 special character</span>

                            <script type="text/javascript">
                                function validate() {
                                    var str = document.getElementById('<%=password.ClientID %>').value;
                                    document.getElementById('<%=password.ClientID %>').style.borderColor = "";

                                    if (str.length < 12) {
                                        document.getElementById("twelvechars").innerHTML = '❌ Password Length must be at least 12 characters';
                                        document.getElementById("twelvechars").style.color = "Red";
                                        document.getElementById('<%=password.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("twelvechars").innerHTML = '✔️ Password Length must be at least 12 characters';
                                        document.getElementById("twelvechars").style.color = "Green";
                                    }

                                    if (str.search(/[a-z]/) == -1) {
                                        document.getElementById("lowercase").innerHTML = '❌ Password require at least 1 lower case alphabet';
                                        document.getElementById("lowercase").style.color = "Red";
                                        document.getElementById('<%=password.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("lowercase").innerHTML = '✔️ Password require at least 1 lower case alphabet';
                                        document.getElementById("lowercase").style.color = "Green";
                                    }

                                    if (str.search(/[A-Z]/) == -1) {
                                        document.getElementById("uppercase").innerHTML = '❌ Password require at least 1 upper case alphabet';
                                        document.getElementById("uppercase").style.color = "Red";
                                        document.getElementById('<%=password.ClientID %>').style.borderColor = "red";
                                    }
                                    else {
                                        document.getElementById("uppercase").innerHTML = '✔️ Password require at least 1 upper case alphabet';
                                        document.getElementById("uppercase").style.color = "Green";
                                    }

                                    if (str.search(/[0-9]/) == -1) {
                                        document.getElementById("numbers").innerHTML = '❌ Password require at least 1 number';
                                        document.getElementById("numbers").style.color = "Red";
                                        document.getElementById('<%=password.ClientID %>').style.borderColor = "red";
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
                    </div>
                    <div class="row justify-content-between text-left">
                        <div class="form-group col-sm-6 flex-column d-flex"> <label class="form-control-label px-3">First name<span class="text-danger"> *</span></label>&nbsp;
                            <asp:TextBox ID="fname" onblur="checkFname();" runat="server"></asp:TextBox>
                            <span id="fnameError" class="text-danger" style="display:none;">First name not valid.</span>
                        </div>
                        <div class="form-group col-sm-6 flex-column d-flex"> <label class="form-control-label px-3">Last name<span class="text-danger"> *</span></label>&nbsp;
                            <asp:TextBox ID="lname" onblur="checkLname();" runat="server"></asp:TextBox>
                            <span id="lnameError" class="text-danger" style="display:none;">Last name not valid.</span>
                            <script>
                                var namepattern = /^[a-zA-Z.+'-]+(?:\s[a-zA-Z.+'-]+)*\s?$/;
                                function checkFname() {
                                    var str = document.getElementById('<%=fname.ClientID %>').value;

                                    document.getElementById('fnameError').style.display = "none";
                                    document.getElementById('<%=fname.ClientID %>').style.borderColor = "";

                                    if (namepattern.test(str) == false) {
                                        document.getElementById('fnameError').style.display = "block";
                                        document.getElementById('<%=fname.ClientID %>').style.borderColor = "red";
                                    }
                                }

                                function checkLname() {
                                    var str = document.getElementById('<%=lname.ClientID %>').value;

                                    document.getElementById('lnameError').style.display = "none";
                                    document.getElementById('<%=lname.ClientID %>').style.borderColor = "";

                                    if (!namepattern.test(str)) {
                                        document.getElementById('lnameError').style.display = "block";
                                        document.getElementById('<%=lname.ClientID %>').style.borderColor = "red";
                                    }
                                }
                            </script>
                        </div>
                    </div>
                    
                    <div class="row justify-content-between text-left">
                        <div class="form-group col-sm-6 flex-column d-flex"> <label class="form-control-label px-3"><span> Credit Card Number *</span></label>&nbsp;
                            <asp:TextBox ID="ccard" inputmode="numeric" pattern="[0-9\s]{13,19}" autocomplete="cc-number" maxlength="19" placeholder="xxxx xxxx xxxx xxxx" onblur="checkCardNumber()" runat="server"></asp:TextBox>
                            <span id="ccardError" class="text-danger" style="display:none;">Invalid Credit Card</span>
                        </div> 
                        <div class="form-group col-sm-3 flex-column d-flex"> <label class="form-control-label px-3"><span> Expiry *</span></label>&nbsp;
                            <asp:TextBox ID="expiry" inputmode="numeric" autocomplete="cc-expiry" maxlength="5" placeholder="MM/YY" onkeydown="checkExpiry()" onchange="checkExpiry()" runat="server"></asp:TextBox>
                            <span id="expiryError" class="text-danger" style="display:none;">Invalid Expiry Date</span>
                        </div>   
                        <div class="form-group col-sm-3 flex-column d-flex"> <label class="form-control-label px-3"><span> CVV *</span></label>&nbsp;
                            <asp:TextBox ID="cvv" inputmode="numeric" autocomplete="cc-cvc" maxlength="3" placeholder="CVV" runat="server" onchange="checkCVC()"></asp:TextBox>
                            <span id="cvvError" class="text-danger" style="display:none;">Invalid CVV</span>
                        </div> 
                        <script>
                            function checkCardNumber() {
                                var pattern = /^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$/;
                                var ccardError = document.getElementById('ccardError');
                                var ccard = document.getElementById('<%=ccard.ClientID %>').value;
                                ccardError.style.display = "none";

                                if (pattern.test(ccard) == false)
                                    ccardError.style.display = "block";

                            }

                            function checkCVC() {
                                console.log("im in");
                                var pattern = /^[0-9]{3}$/;
                                var cvvError = document.getElementById('cvvError');
                                var cvv = document.getElementById('<%=cvv.ClientID %>').value;
                                cvvError.style.display = "none";

                                if (pattern.test(cvv) == false)
                                    cvvError.style.display = "block";
                                else
                                    console.log("nth wrong")
                            }

                            function checkExpiry() {
                                var expiryError = document.getElementById('expiryError');
                                var expiry = document.getElementById('<%=expiry.ClientID %>').value;
                                expiryError.style.display = "none";

                                var today = new Date();
                                var mm = (today.getMonth() + 1).toString();
                                var yy = today.getFullYear().toString().substr(-2);

                                var result = false;
                                expiryArray = expiry.split('/');
                                var pattern = /^\d{2}$/;

                                if (expiryArray[0] < 1 || expiryArray[0] > 12)
                                    result = true;

                                if (!pattern.test(expiryArray[0]) || !pattern.test(expiryArray[1]))
                                    result = true;

                                if (expiryArray[2])
                                    result = true;

                                if (parseInt(expiryArray[1]) < yy)
                                    result = true;

                                if (parseInt(expiryArray[0]) <= mm && parseInt(expiryArray[1]) == yy)
                                    result = true;

                                if (result)
                                    expiryError.style.display = "block";
                            }
                        </script>
                    </div>
                    <div class="row justify-content-between text-left">
                        <div class="form-group col-6 flex-column d-flex"> <label class="form-control-label px-3">Date of Birth<span class="text-danger"> *</span></label>&nbsp; 
                            <asp:TextBox ID="dob" TextMode="Date" runat="server" max="2020-01-01" min="1910-01-01" onchange="checkDOB()"></asp:TextBox>
                            <span id="dobError" class="text-danger"></span>
                            
                        </div>
                        <div class="form-group col-6 flex-column d-flex"> <label class="form-control-label px-3">Photo
                            <span class="text-danger"> *</span></label>&nbsp; 
                            <asp:FileUpload ID="imgUpload" runat="server" accept="image/*" onchange="ValidateExtension()"/>
                            <span class="text-danger" id="imguploadError"></span>
                            <script>
                                function ValidateExtension() {
                                    var allowedFiles = [".jpg"];
                                    var fileUpload = document.getElementById('<%=imgUpload.ClientID %>');
                                    var lblError = document.getElementById("imguploadError");
                                    var regex = new RegExp("([a-zA-Z0-9\s_\\.\-:])+(" + allowedFiles.join('|') + ")$");
                                    if (!regex.test(fileUpload.value.toLowerCase())) {
                                        lblError.innerHTML = "Please upload files having extensions: <b>" + allowedFiles.join(', ') + "</b> only.";
                                        document.getElementById('<%=imgUpload.ClientID %>').value = "";
                                        return false;
                                    }
                                    else if (fileUpload.files[0].size > (200 *1024)) {
                                        lblError.innerHTML = "File size should not be more than 0.2mb!</b>";
                                        document.getElementById('<%=imgUpload.ClientID %>').value = "";
                                        return false;
                                    }
                                    lblError.innerHTML = "";
                                    return true;
                                }
                            </script>
                        </div>
                    </div>
                    <div class="row justify-content-end">
                        <div class="form-group col-sm-12"> 
                            <asp:Label ID="lbl_pwdchecker" runat="server"></asp:Label>
                            <asp:Button ID="submit" class="btn-block btn-primary" runat="server" Text="Submit" OnClick="submit_Click" />
                        </div>
                    </div>
                    <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
                </form>
            </div>
        </div>
    </div>
</div>
</body>
</html>

