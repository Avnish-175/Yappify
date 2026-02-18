// Dashboard.js - Manages all client-side logic for the chat dashboard

$(document).ready(function () {

    // 📦 DOM REFERENCES
    const messageInput = $("#messageInput");
    const sendButton = $("#sendButton");
    const chatBody = $(".chat-body");
    const toggleBtn = $("#toggleSidebar");
    const sidebar = $("#sidebar");
    const chatHeader = $("#chatHeader");
    const myProfilePicPreview = $("#myProfilePicPreview");
    const profilePicInput = $("#profilePicInput");
    const onlineStatusVisibility = $("#onlineStatusVisibility");
    const aboutMeStatus = $("#aboutMeStatus");
    const saveProfileSettingsBtn = $("#saveProfileSettings");
    const myProfileSidebarPic = $("#profileToggle");
    const currentUserId = localStorage.getItem("userId");

    // 📌 STATE
    let currentChatUserId = null;

    // ⏰ HELPER FUNCTIONS
    function formatTimestamp(date) {
        let hours = date.getHours();
        const minutes = date.getMinutes();
        const ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12 || 12;
        const mins = minutes < 10 ? '0' + minutes : minutes;
        return `${hours}:${mins} ${ampm}`;
    }

    function updateChatHeader(name, pic, status = "Online") {
        chatHeader.find(".chat-user-pic").attr("src", pic);
        chatHeader.find(".chat-username").text(name);
        chatHeader.find(".chat-status").text(status);
    }

    function appendMessageToChat(msg, senderType, timestamp, messageType = "text", fileUrl = "") {
        const formattedTime = formatTimestamp(new Date(timestamp || Date.now()));
        let html = `<div class="message ${senderType}">`;

        if (messageType === "text") {
            html += `${msg}`;
        } else if (messageType === "image") {
            html += `<img src="${fileUrl}" alt="image" class="chat-image"/>`;
        } else if (messageType === "file") {
            html += `<a href="${fileUrl}" target="_blank" class="chat-file">${msg || 'Download File'}</a>`;
        }

        html += `<span class="timestamp">${formattedTime}</span></div>`;
        chatBody.append(html);
        chatBody.scrollTop(chatBody[0].scrollHeight);
    }

    // 💾 LOAD PROFILE SETTINGS
    function loadProfileSettings() {
        const myProfilePicPreview = $("#myProfilePicPreview");

        $.ajax({
            url: "/api/Username/MyProfile",
            method: "GET",
            success: function (data) {
                if (data.profilePicUrl) {
                    myProfilePicPreview.attr("src", data.profilePicUrl);

                    myProfilePicPreview.on("error", function () {
                        $(this).replaceWith('<i class="chat-user-pic fa fa-user-circle fa-3x" aria-hidden="true" id="myProfilePicPreview"></i>');
                    });

                    localStorage.setItem("myProfilePic", data.profilePicUrl);
                }

                if (data.about) {
                    aboutMeStatus.val(data.about);
                    localStorage.setItem("myAboutMeStatus", data.about);
                }
            },
            error: function () {
                console.warn("Failed to load profile from DB, falling back to localStorage.");
            }
        });
    }


    // 🫂 FRIEND LIST MANAGEMENT
    function loadFriendsList() {
        $.ajax({
            url: "/api/FriendRequest/Friends",
            method: "GET",
            success: function (friends) {
                const container = $("#friendsList");
                container.empty();

                if (!friends || friends.length === 0) {
                    container.html("<p>No friends yet.</p>");
                    return;
                }

                friends.forEach(friend => {
                    const id = parseInt(friend.userId);
                    if (isNaN(id)) {
                        console.warn("Skipping invalid friend. Raw data:", JSON.stringify(friend, null, 2));
                        return;
                    }
                    container.append(`
                        <div class="friend-card" data-id="${id}">
                            <img src="${friend.profilePicUrl || '/images/default-avatar.png'}" class="profile-pic" />
                            <div class="user-info">
                                <div class="username">@${friend.publicUsername}</div>
                                <div class="bio">${friend.aboutMe || ''}</div>
                            </div>
                        </div>`);
                });
            },
            error: function (err) {
                console.error("Failed to load friends:", err);
            }
        });
    }

    // 📥 FRIEND REQUEST MANAGEMENT
    function loadReceivedRequests() {
        $.ajax({
            url: "/api/FriendRequest/Received",
            type: "GET",
            success: function (requests) {
                const container = $("#receivedRequests");
                container.empty();
                if (requests.length === 0) {
                    container.html("<p>No friend requests</p>");
                    return;
                }
                requests.forEach(req => {
                    container.append(`
                        <div class="request-card">
                            <img src="${req.profilePicUrl || '/images/default-avatar.png'}" class="profile-pic" />
                            <div class="user-info">
                                <div class="username">@${req.publicUsername}</div>
                                <div class="bio">${req.aboutMe || ''}</div>
                            </div>
                            <div class="request-actions">
                                <button class="btn-accept" data-id="${req.senderId}">Accept</button>
                                <button class="btn-reject" data-id="${req.senderId}">Reject</button>
                            </div>
                        </div>`);
                });
            },
            error: function (err) {
                console.error("Failed to load requests", err);
            }
        });
    }

    // 🔌 SIGNALR CHAT CONNECTION
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .build();

    connection.on("ReceiveMessage", function (fromUserId, message, messageType, fileUrl) {
        if (fromUserId === currentChatUserId) {
            appendMessageToChat(message, "received", null, messageType, fileUrl);
        } else {
            console.log("New message from other user (not active chat)");
        }
    });

    connection.start()
        .then(() => {
            if (currentUserId) {
                connection.invoke("JoinUserGroup", currentUserId);
                console.log("SignalR connected and joined user group:", currentUserId);
            } else {
                console.warn("SignalR connected but userId not found for joining group.");
            }
        })
        .catch(err => console.error("SignalR error:", err));

    // ➡️ EVENT HANDLERS

    // Send message via button click
    sendButton.click(function () {
        const msg = messageInput.val().trim();

        // Client-side check to prevent sending empty messages
        if (!msg) {
            console.warn("Message is empty, not sending.");
            return;
        }

        // Ensure IDs are valid integers before proceeding
        const senderId = parseInt(currentUserId);
        const receiverId = parseInt(currentChatUserId); // Ensure this is also an int

        if (isNaN(senderId) || isNaN(receiverId)) {
            console.error("Invalid sender or receiver ID. Please select a valid chat.");
            return; // Stop the function here
        }

        // The rest of the code is the same
        const now = new Date();
        appendMessageToChat(msg, "sent", now, "text", "");
        messageInput.val("");

        const payload = {
            UserId: senderId,
            ReceiverId: receiverId,
            Message: msg,
            MessageType: "text",
            FileUrl: ""
        };

        // Log the payload to confirm the data is correct before sending
        console.log("Sending payload:", payload);

        // Save to DB
        $.ajax({
            url: "/api/Message/SaveMessage",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: () => console.log("Message saved successfully."),
            error: (xhr, status, error) => {
                console.error("Message DB error:", xhr.status, xhr.responseText);
                // You can add more detailed logging here if needed
            }
        });

        // Send real-time via SignalR
        connection.invoke("SendMessage", senderId, receiverId, msg, "text", "")
            .catch(err => console.error("SignalR send error:", err));
    });;

    // Send message on Enter key
    messageInput.keypress(function (e) {
        if (e.which === 13) sendButton.click();
    });

    // Toggle sidebar
    toggleBtn.click(() => sidebar.toggleClass("collapsed active"));

    // Tab navigation
    $(".nav-tabs .tab").click(function () {
        $(".tab-panel").removeClass("active");
        $("#" + $(this).data("tab") + "-panel").addClass("active");
        $(".tab").removeClass("active");
        $(this).addClass("active");
    });

    // Profile picture file preview
    profilePicInput.change(function () {
        const file = this.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = e => myProfilePicPreview.attr("src", e.target.result);
        reader.readAsDataURL(file);
    });

    // Save profile settings
    saveProfileSettingsBtn.click(function () {
        const profilePicBase64 = myProfilePicPreview.attr("src");
        const statusVisibility = onlineStatusVisibility.val();
        const aboutMe = aboutMeStatus.val().trim();
        localStorage.setItem("myProfilePic", profilePicBase64);
        localStorage.setItem("myOnlineStatusVisibility", statusVisibility);
        localStorage.setItem("myAboutMeStatus", aboutMe);
        myProfileSidebarPic.attr("src", profilePicBase64);
        $.ajax({
            url: "/api/Username/SaveProfileDetails",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                AboutMe: aboutMe,
                ProfilePicBase64: profilePicBase64,
                OnlineStatus: statusVisibility
            }),
            success: () => alert("Profile settings saved!"),
            error: err => alert("Failed to save to server.")
        });
    });

    // Logout
    $("#logoutButton").click(() => {
        localStorage.clear();
        window.location.href = "/Home/Index";
    });

    // Live friend search
    $("#userSearchInput").on("input", function () {
        const query = $(this).val().trim();
        if (query.length < 2) return $("#searchResults").empty();
        $.ajax({
            url: `/api/Username/SearchByUsername?query=${encodeURIComponent(query)}`,
            method: "GET",
            success: function (users) {
                const searchResults = $("#searchResults").empty();
                if (users.length === 0) return searchResults.html("<p>No users found</p>");
                users.forEach(user => {
                    let btnText = "Add Friend", btnDisabled = "";
                    if (user.friendStatus === "Pending") { btnText = "Requested"; btnDisabled = "disabled"; }
                    else if (user.friendStatus === "Accepted") { btnText = "Friends"; btnDisabled = "disabled"; }
                    else if (user.friendStatus === "Received") { btnText = "Request Received"; btnDisabled = "disabled"; }
                    searchResults.append(`
                        <div class="user-profile-card">
                            <img src="${user.profilePicUrl || '/images/default-avatar.png'}" class="profile-pic" />
                            <div class="user-details">
                                <div class="username">@${user.publicUsername}</div>
                                <div class="bio">${user.about || ''}</div>
                            </div>
                            <button class="send-request-btn" data-id="${user.userId}" ${btnDisabled}>${btnText}</button>
                        </div>`);
                });
            },
            error: err => {
                $("#searchResults").html("<p>Error searching users</p>");
                console.error("Search error:", err);
            }
        });
    });

    // Send friend request
    $(document).on("click", ".send-request-btn:not(:disabled)", function () {
        const toUserId = $(this).data("id");
        const $btn = $(this);
        $.ajax({
            url: "/api/FriendRequest/Send",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ toUserId }),
            success: () => $btn.text("Requested").prop("disabled", true),
            error: err => {
                alert("Could not send request");
                console.error(err);
            }
        });
    });

    // Accept friend request
    $(document).on("click", ".btn-accept", function () {
        const toUserId = $(this).data("id");
        $.ajax({
            url: "/api/FriendRequest/Accept",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ toUserId }),
            success: function () {
                loadReceivedRequests();
                loadFriendsList();
            },
            error: function (err) {
                console.error("Accept error:", err);
                alert("Failed to accept request.");
            }
        });
    });

    // Reject friend request
    $(document).on("click", ".btn-reject", function () {
        const toUserId = $(this).data("id");
        $.ajax({
            url: "/api/FriendRequest/Reject",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ toUserId }),
            success: function () {
                loadReceivedRequests();
            },
            error: function (err) {
                console.error("Reject error:", err);
                alert("Failed to reject request.");
            }
        });
    });

    // Handle friend click to start chat
    $(document).on("click", ".friend-card", function () {
        const friendId = parseInt($(this).data("id"));
        if (isNaN(friendId)) {
            console.warn("Invalid friend ID clicked. data-id is missing or malformed.");
            return;
        }

        const username = $(this).find(".username").text().trim().replace("@", "");
        const profilePic = $(this).find("img").attr("src");

        currentChatUserId = friendId;
        localStorage.setItem("activeChatUserId", friendId);

        updateChatHeader(username, profilePic);
        console.log("Fetching chat history between:", currentUserId, friendId);

        // Load chat history
        $.ajax({
            url: `/api/Message/History?user1=${currentUserId}&user2=${friendId}`,
            method: "GET",
            success: function (messages) {
                chatBody.empty();
                messages.forEach(m => {
                    const senderType = m.sentByMe ? "sent" : "received";
                    appendMessageToChat(m.message, senderType, m.time, m.messageType, m.fileUrl);
                });
                chatBody.scrollTop(chatBody[0].scrollHeight);
            },
            error: function (err) {
                alert("Failed to load chat history.");
                console.error("Chat history error:", err);
            }
        });

        $(".tab-panel").removeClass("active");
        $("#chats-panel").addClass("active");
    });

    // 🔃 INITIALIZATION
    loadProfileSettings();
    loadFriendsList();

    function loadChatMessages(currentUserId, friendUserId) {
        $.ajax({
            url: `/api/Chat/GetMessages?currentUserId=${currentUserId}&friendUserId=${friendUserId}`,
            method: "GET",
            success: function (messages) {
                const chatBody = $(".chat-body");
                chatBody.empty();

                messages.forEach(msg => {
                    const alignment = msg.CreatedBy == currentUserId ? "sent" : "received";
                    const messageHtml = `
                    <div class="message ${alignment}">
                        ${msg.Text}
                        <span class="timestamp">${msg.Timestamp}</span>
                    </div>`;
                    chatBody.append(messageHtml);
                });
            },
            error: function () {
                console.error("Failed to load messages.");
            }
        });
    }

});