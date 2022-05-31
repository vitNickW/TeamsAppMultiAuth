let accessToken;
let returnUrl = $("#returnUrl").data("returnurl");

$(document).ready(function () {
    microsoftTeams.initialize();

    getClientSideToken()
        .then((clientSideToken) => {
            return getServerSideToken(clientSideToken);
        })
        .catch((error) => {
            console.log(error);
            if (error === "invalid_grant") {
                // Display in-line button so user can consent
                $("#divError").text("Error while exchanging for Server token - invalid_grant - User or admin consent is required.");
                $("#divError").show();
                $("#consent").show();
            } else {
                // Something else went wrong
            }
        });
});

function requestConsent() {
    getToken()
        .then(data => {
            $("#consent").hide();
            $("#divError").hide();
            accessToken = data.accessToken;
            microsoftTeams.getContext((context) => {
                getUserInfo(context.userPrincipalName);
            });
        });
}

function getToken() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/Auth/Start",
            width: 600,
            height: 535,
            successCallback: result => {

                resolve(result);
            },
            failureCallback: reason => {

                reject(reason);
            }
        });
    });
}

function getClientSideToken() {
    console.log('Fetching client-side token');

    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.getAuthToken({
            successCallback: (result) => {
                resolve(result);

            },
            failureCallback: function (error) {
                reject("Error getting token: " + error);
            }
        });

    });

}

function getServerSideToken(clientSideToken) {
    console.log('Fetching server-side token');

    return new Promise((resolve, reject) => {
        microsoftTeams.getContext((context) => {
            var scopes = ["https://graph.microsoft.com/User.Read"];
            console.log("Fetching GetUserAccessToken from Home Controller");

            fetch('/loginteamssso', {
                method: 'get',
                headers: {
                    "Content-Type": "application/text",
                    "Authorization": "Bearer " + clientSideToken
                },
                cache: 'default'
            })
            //.then((response) => {
            //    debugger;
            //    window.location.assign(window.origin + "/Home/Secured");
            //});
                .then((response) => {
                    console.log("First then of getServerSideToken");
                    
                    debugger;
                    if (response.ok) {
                        
                        window.location.assign(window.origin + "/Home/Secured");
                    } else {
                        
                        window.location.assign(window.origin + "/denied");
                    }
                })
                //.then((responseJson) => {
                //    console.log("Second then of getServerSideToken");

                //    if (IsValidJSONString(responseJson)) {
                //        if (JSON.parse(responseJson).error)
                //            reject(JSON.parse(responseJson).error);
                //    } else if (responseJson) {
                //        accessToken = responseJson;
                //        getUserInfo(context.userPrincipalName);
                //        //window.location.assign("/Home/Secured");
                //    }
                //});
        });
    });
}

function IsValidJSONString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}


function getUserInfo(principalName) {
    if (principalName) {
        let graphUrl = "https://graph.microsoft.com/v1.0/users/" + principalName;
        $.ajax({
            url: graphUrl,
            type: "GET",
            beforeSend: function (request) {
                request.setRequestHeader("Authorization", `Bearer ${accessToken}`);
            },
            success: function (profile) {
                let profileDiv = $("#divGraphProfile");
                profileDiv.empty();
                for (let key in profile) {
                    if ((key[0] !== "@") && profile[key]) {
                        $("<div>")
                            .append($("<b>").text(key + ": "))
                            .append($("<span>").text(profile[key]))
                            .appendTo(profileDiv);
                    }
                }
                $("#divGraphProfile").show();
            },
            error: function () {
                console.log("Failed");
            },
            complete: function (data) {
            }
        });
    }
}