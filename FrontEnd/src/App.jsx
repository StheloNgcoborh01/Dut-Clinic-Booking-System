import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './styles/Auth.css';
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';
import VerifyForm from './components/VerifyForm';
import ForgotPassword from './components/ForgotPassword';
import VerifyForgot from './components/VerifyForgot';
import PasswordReset from './components/PasswordReset';
import ProtectedRoute from './services/ProtectedRoute';
import NavBar from './components/NavBar';
import Home from './pages/Home';
import './styles/Navbar.css';
import AddBooking from './pages/AddBooking.jsx';
import MyBookings from './pages/MyBookings';
import Contact from './pages/Contact';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminRoute from './services/AdminRoute';
import TodaysBookings from './pages/admin/TodaysBookings';
import AllBookings from './pages/admin/AllBookings';
import Messages from './pages/admin/Messages';
import Users from './pages/admin/Users';






function App() {
  return (
    <BrowserRouter>
      <div className="App">
        <Routes>

          <Route path="/login" element={ <LoginForm /> } />
          <Route path="/register" element={<RegisterForm />} />
          <Route path="/verify" element={<VerifyForm />} />
          <Route path="/forgot" element={<ForgotPassword />} />
          <Route path="/verifyForgot" element={<VerifyForgot />} />
          <Route path ="passwordReset" element = { <PasswordReset /> } /> 
          <Route path ="home" element = {  <Home /> } />
          <Route path = "/" element = { <Home /> } />
          <Route path="/contact" element={<Contact />} />

          <Route path ="AddBooking" element =  {<ProtectedRoute> <AddBooking /> </ProtectedRoute> }    /> 
          <Route path="/bookings" element=    { <ProtectedRoute>  <MyBookings />   </ProtectedRoute> } />
        <Route path="/admin/dashboard" element= { <ProtectedRoute> <AdminDashboard /> </ProtectedRoute>  } />
        <Route path="/admin/users" element={    <AdminRoute>   <Users />   </AdminRoute> }/>

        <Route path="/admin/dashboard" element={ <AdminRoute>  <AdminDashboard />  </AdminRoute>  }/>
        <Route  path="/admin/todays-bookings" element={ <AdminRoute>  <TodaysBookings />  </AdminRoute> }/>
        <Route path="/admin/all-bookings" element={ <AdminRoute> <AllBookings /> </AdminRoute> }/>
       <Route path="/admin/messages"  element={  <AdminRoute>    <Messages />  </AdminRoute>  }/>
       
        </Routes>
      </div>
    </BrowserRouter>
  );
}


export default App;