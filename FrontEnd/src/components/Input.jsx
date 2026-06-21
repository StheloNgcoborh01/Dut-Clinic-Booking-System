function Input({ label, type, placeholder, id, value, onChange }) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
       
      />
    </div>
  );
}

export default Input;