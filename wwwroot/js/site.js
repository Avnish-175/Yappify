
document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('container');

    if (!container) return; // Not a login/signup page, skip toggle

    window.toggle = function () {
        container.classList.toggle('sign-in');
        container.classList.toggle('sign-up');
    };

    setTimeout(() => {
        container.classList.add('sign-in');
    }, 200);
});





// SIGN UP
$(document).ready(function () {
    $("#signUpBtn").click(function (e) {
        e.preventDefault();

        // Clear old errors
        $(".error-message").text("");

        const phone = $("#phone").val().trim();
        const email = $("#email").val().trim();
        const password = $("#password").val();
        const confirmPassword = $("#confirmPassword").val();
        const name = $("#username").val().trim();

        let hasError = false;

        if (!name) {
            $("#usernameError").text("Username is required.");
            hasError = true;
        }
        if (!phone) {
            $("#phoneError").text("Phone number is required.");
            hasError = true;
        }
        if (!email) {
            $("#emailError").text("Email is required.");
            hasError = true;
        } else {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                $("#emailError").text("Invalid email format.");
                hasError = true;
            }
        }
        if (!password) {
            $("#passwordError").text("Password is required.");
            hasError = true;
        } else {
            const strengthRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?#&]).{6,}$/;
            if (!strengthRegex.test(password)) {
                $("#passwordError").text("Password must include uppercase, lowercase, number, and special character.");
                hasError = true;
            }
        }

        if (!confirmPassword) {
            $("#confirmPasswordError").text("Please confirm your password.");
            hasError = true;
        } else if (password !== confirmPassword) {
            $("#confirmPasswordError").text("Passwords do not match.");
            hasError = true;
        }

        if (hasError) return;

        var userData = {
            PhoneNo: phone,
            EmailID: email,
            Password: password,
            Name: name
        };

        $.ajax({
            url: "/api/Login/SignUp",
            type: "POST",
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    window.location.href = "/Welcome/Index";
                } else {
                    alert("Signup failed: " + response.message);
                }
            },
            error: function (xhr) {
                alert("Signup failed: " + xhr.responseText);
            }
        });
    });

    // LOGIN
    $("#loginBtn").click(function (e) {
        e.preventDefault();

        $("#loginEmailError").text("");
        $("#loginPasswordError").text("");

        const email = $("#txtUserID").val().trim();
        const password = $("#txtPassword").val();

        let hasError = false;

        if (!email) {
            $("#loginEmailError").text("Email is required.");
            hasError = true;
        } else {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                $("#loginEmailError").text("Enter a valid email.");
                hasError = true;
            }
        }

        if (!password) {
            $("#loginPasswordError").text("Password is required.");
            hasError = true;
        }

        if (hasError) return;

        var loginRequest = {
            EmailID: email,
            Password: password
        };


        $.ajax({
            url: "/api/Login/Login",
            type: "POST",
            data: JSON.stringify(loginRequest),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                if (res.success) {
                    // ✅ Store userId in localStorage
                    localStorage.setItem("userId", res.data.id);

                    $("#loginErrorBox").hide();
                    window.location.href = "/Login/RouteUser";
                } else {
                    $("#loginErrorBox").text(res.message || "Login failed.").show();
                    setTimeout(() => {
                        $("#loginErrorBox").fadeOut();
                    }, 5000);
                }
            },
            error: function (xhr) {
                let errorMsg = "Login failed. Please try again.";
                try {
                    const res = JSON.parse(xhr.responseText);
                    if (res.message) errorMsg = res.message;
                } catch (e) { }
                $("#loginErrorBox").text(errorMsg).show();
                setTimeout(() => {
                    $("#loginErrorBox").fadeOut();
                }, 5000);
             }

        });
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const togglePwd = document.getElementById("togglePassword");
    const pwdInput = document.getElementById("password");

    if (togglePwd && pwdInput) {
        togglePwd.addEventListener("click", function () {
            const type = pwdInput.type === "password" ? "text" : "password";
            pwdInput.type = type;
            this.classList.toggle("bx-show");
            this.classList.toggle("bx-hide");
        });
    }

    const toggleConfirmPwd = document.getElementById("toggleConfirmPassword");
    const confirmPwdInput = document.getElementById("confirmPassword");

    if (toggleConfirmPwd && confirmPwdInput) {
        toggleConfirmPwd.addEventListener("click", function () {
            const type = confirmPwdInput.type === "password" ? "text" : "password";
            confirmPwdInput.type = type;
            this.classList.toggle("bx-show");
            this.classList.toggle("bx-hide");
        });
    }
});

