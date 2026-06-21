function Checkbox({ id, label, checked, onChange }) {
  return (
    <div className="checkbox">
      <input
        id={id}
        type="checkbox"
        checked={checked}
        onChange={onChange}
      />
      <label htmlFor={id}>{label}</label>
    </div>
  );
}

export default Checkbox;