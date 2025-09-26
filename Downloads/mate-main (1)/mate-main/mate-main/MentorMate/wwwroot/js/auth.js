// ========== SIGN UP LOGIC ========== //
const steps = document.querySelectorAll(".step");
const nextBtns = document.querySelectorAll(".btn-next");
const prevBtns = document.querySelectorAll(".btn-prev");
const progressBar = document.getElementById("progressBar");
const roleCards = document.querySelectorAll(".role-card");
const roleInput = document.getElementById("role");
const extraFields = document.getElementById("extraFields");
let currentStep = 0;

function showStep(n) {
    steps.forEach((s, i) => s.classList.toggle("active", i === n));
    if (progressBar) progressBar.style.width = ((n + 1) / steps.length * 100) + "%";
}

if (nextBtns) {
    nextBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            if (currentStep < steps.length - 1) {
                currentStep++;
                showStep(currentStep);
            }
        });
    });
}

if (prevBtns) {
    prevBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            if (currentStep > 0) {
                currentStep--;
                showStep(currentStep);
            }
        });
    });
}

if (roleCards) {
    roleCards.forEach(card => {
        card.addEventListener("click", () => {
            roleCards.forEach(c => c.classList.remove("border-primary", "bg-light"));
            card.classList.add("border-primary", "bg-light");
            roleInput.value = card.dataset.role;
            if (card.dataset.role === "mentor") {
                extraFields.innerHTML = `
          <div class="mb-3"><input type="text" class="form-control" placeholder="Expertise" required></div>
          <div class="mb-3"><input type="number" class="form-control" placeholder="Years of Experience" required></div>
          <div class="mb-3"><input type="url" class="form-control" placeholder="LinkedIn Profile"></div>
        `;
            } else {
                extraFields.innerHTML = `
          <div class="mb-3"><input type="text" class="form-control" placeholder="Field of Study" required></div>
          <div class="mb-3"><input type="text" class="form-control" placeholder="Interests"></div>
        `;
            }
        });
    });
}

showStep(currentStep);

// ========== LOGIN LOGIC ========== //
document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const data = {
                email: document.getElementById("loginEmail").value,
                password: document.getElementById("loginPassword").value,
                rememberMe: document.getElementById("rememberMe").checked
            };
            console.log("Login Data:", data);
            // fetch("/api/login", { ... })  <-- جاهز للباك
            alert("Login successful! (Mock)");
        });
    }

    // FORGOT PASSWORD LOGIC
    const forgotLink = document.getElementById("forgotPasswordLink");
    const forgotForm = document.getElementById("forgotPasswordForm");
    if (forgotLink) {
        forgotLink.addEventListener("click", (e) => {
            e.preventDefault();
            const modal = new bootstrap.Modal(document.getElementById("forgotModal"));
            modal.show();
        });
    }
    if (forgotForm) {
        forgotForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const email = document.getElementById("forgotEmail").value;
            console.log("Forgot Password Email:", email);
            // fetch("/api/forgot-password", { method:"POST", body: JSON.stringify({email}), headers: {"Content-Type":"application/json"} })
            alert("Reset link sent to your email! (Mock)");
            bootstrap.Modal.getInstance(document.getElementById("forgotModal")).hide();
        });
    }

    // RESET PASSWORD LOGIC
    const resetForm = document.getElementById("resetPasswordForm");
    if (resetForm) {
        resetForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const newPass = document.getElementById("newPassword").value;
            const confirmPass = document.getElementById("confirmPassword").value;
            if (newPass !== confirmPass) {
                alert("Passwords do not match!");
                return;
            }
            console.log("Reset Password:", newPass);
            // fetch("/api/reset-password", { method:"POST", body: JSON.stringify({newPass}), headers: {"Content-Type":"application/json"} })
            alert("Password updated successfully! (Mock)");
        });
    }
});

