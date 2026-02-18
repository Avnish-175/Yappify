$(document).ready(function () {
    $("#usernameInput").on("input", function () {
        const username = $(this).val().trim();
        if (username.length < 3) {
            $("#feedback").text("Username too short (min 3 characters).").css("color", "red");
            return;
        }

        if (!/^[a-zA-Z0-9_]{3,15}$/.test(username)) {
            $("#feedback").text("Username must be 3-15 characters, only letters, numbers, or _ allowed.").css("color", "red");
            return;
        }

        clearTimeout($(this).data('timer'));
        $(this).data('timer', setTimeout(function () {
            $.get(`/api/Username/IsAvailable?username=${username}`, function (res) {
                if (res.available) {
                    $("#feedback").text("✅ Available").css("color", "green");
                } else {
                    $("#feedback").text("❌ Taken. Suggestions:").css("color", "red");
                    $.get(`/api/Username/Suggestions?baseUsername=${username}`, function (suggestions) {
                        if ($("#feedback").find('ul').length === 0) {
                            $("#feedback").append("<ul>" + suggestions.map(s => `<li>${s}</li>`).join('') + "</ul>");
                        }
                    });
                }
            }).fail(function (jqXHR) {
                console.error("Availability check failed:", jqXHR);
                $("#feedback").text("Error checking availability. Please try again.").css("color", "red");
            });
        }, 300));
    });

    $("#saveBtn").on("click", function () {
        const username = $("#usernameInput").val().trim();
        if (!username) {
            alert("Please enter a username");
            return;
        }

        if (!/^[a-zA-Z0-9_]{3,15}$/.test(username)) {
            $("#feedback").text("Username must be 3-15 characters, only letters, numbers, or _ allowed.").css("color", "red");
            return;
        }

        $.ajax({
            url: "/api/Username/Save",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ PublicUsername: username }),
            success: function (res) {
                if (res.success) {
                    window.location.href = "/Dashboard/Index";
                } else {
                    const errorMessage = res.message || "Failed to save username. Please try again.";
                    $("#feedback").text("Error: " + errorMessage).css("color", "red");
                }
            },
            error: function (jqXHR) {
                if (jqXHR.status === 401) {
                    $("#feedback").text("Error: Not authenticated. Please log in again.").css("color", "red");
                    setTimeout(() => window.location.href = "/", 2000);
                } else if (jqXHR.status === 409) {
                    const errorResponse = jqXHR.responseJSON;
                    $("#feedback").text("Error: " + (errorResponse ? errorResponse.message : "Username is taken.")).css("color", "red");
                } else {
                    $("#feedback").text("An unknown error occurred. Please try again.").css("color", "red");
                }
                console.error("AJAX Error:", jqXHR);
            }
        });
    });
});
