import React, { useEffect, useState } from 'react';
import Modal from './Modal';

interface TextInputDialogProps {
  open: boolean;
  title: string;
  label: string;
  initialValue: string;
  confirmLabel?: string;
  onCancel: () => void;
  onConfirm: (value: string) => void;
}

const TextInputDialog: React.FC<TextInputDialogProps> = ({
  open,
  title,
  label,
  initialValue,
  confirmLabel = 'Save',
  onCancel,
  onConfirm,
}) => {
  const [value, setValue] = useState(initialValue);

  useEffect(() => {
    if (open) {
      setValue(initialValue);
    }
  }, [initialValue, open]);

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    const trimmed = value.trim();
    if (!trimmed) return;
    onConfirm(trimmed);
  };

  return (
    <Modal open={open} title={title} onClose={onCancel}>
      <form onSubmit={handleSubmit}>
        <div className="px-5 py-4">
          <label className="block text-sm font-medium text-gray-700">
            {label}
            <input
              autoFocus
              value={value}
              onChange={(event) => setValue(event.target.value)}
              className="input mt-2"
            />
          </label>
        </div>
        <div className="flex justify-end gap-2 border-t border-gray-100 px-5 py-4">
          <button type="button" onClick={onCancel} className="btn-secondary">
            Cancel
          </button>
          <button type="submit" disabled={!value.trim()} className="btn-primary">
            {confirmLabel}
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default TextInputDialog;
