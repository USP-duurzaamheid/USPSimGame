document.addEventListener("submit", function (event) {
  if (event.target.id !== "loginForm") {
    return;
  }

  const button = document.getElementById("loginButton");
  const buttonText = document.getElementById("loginButtonText");

  if (button && buttonText) {
    button.disabled = true;

    buttonText.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"></span>
                Authenticating...
            `;
  }
});
