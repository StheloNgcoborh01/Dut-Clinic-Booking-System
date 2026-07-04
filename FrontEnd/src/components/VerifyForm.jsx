import { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import Input from "./Input";
import Button from "./Button";
import { useNavigate } from "react-router-dom";
import { isLoggedIn } from "../services/AuthServices";

function VerifyForm() {
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    if (isLoggedIn()) {
      navigate("/dashboard");
    }
  }, [navigate]);

  // Read email from URL when component loads
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const emailFromUrl = params.get("email");
    if (emailFromUrl) {
      setEmail(emailFromUrl);
    }
  }, [location]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      const response = await fetch("http://localhost:3000/api/Auth/Verify", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          Email: email,

          VerifyCode: code,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        setTimeout(
          setMessage("Email verified successfully! Redirecting to login..."),
          5000,
        );

        localStorage.setItem("token", data.token);
        navigate("/login");
      } else {
        setMessage(data.message || "Verification failed. Please try again.");
      }
    } catch (error) {
      setMessage("Network error. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const HandleResend = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      const response = await fetch(
        "https://dut-clinic-booking-system-y9d7.onrender.com/api/Auth/ResendEmail",
        {
          method: "PATCH",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            Email: email,
          }),
        },
      );

      const data = await response.json();

      if (response.ok) {
         navigate('/verify?email=' + code);


      } else {
        setMessage(data.message || "Please try again.");
      }
    } catch (error) {
      setMessage("Network error. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-box">
      <h2>Verify Your Email</h2>
      <p
        style={{
          fontSize: "0.85rem",
          color: "#6c7e97",
          marginBottom: "1.5rem",
        }}
      >
        Enter the 6-digit code sent to <strong>{email || "your email"}</strong>
      </p>

      <form onSubmit={handleSubmit}>
        <Input
          label="Verification Code"
          type="text"
          placeholder="123456"
          id="code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
        />

        {message && (
          <div style={{ margin: "0.5rem 0", fontSize: "0.85rem" }}>
            {message}
          </div>
        )}

        <Button type="submit" disabled={loading}>
          {loading ? "Verifying..." : "Verify"}
        </Button>

        <div className="signup-link">
          Didn't receive code? <a onClick={HandleResend}>Resend</a>
        </div>

        <hr />
        <div className="clinic-footer">Ntuzuma Clinic</div>
      </form>
    </div>
  );
}

export default VerifyForm;
