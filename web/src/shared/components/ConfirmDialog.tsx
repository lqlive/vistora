import React from 'react';
import Modal from './Modal';

interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  destructive?: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  title,
  message,
  confirmLabel = 'Confirm',
  destructive = false,
  onCancel,
  onConfirm,
}) => (
  <Modal open={open} title={title} onClose={onCancel}>
    <div className="px-5 py-4">
      <p className="text-sm text-gray-600">{message}</p>
    </div>
    <div className="flex justify-end gap-2 border-t border-gray-100 px-5 py-4">
      <button type="button" onClick={onCancel} className="btn-secondary">
        Cancel
      </button>
      <button
        type="button"
        onClick={onConfirm}
        className={
          destructive
            ? 'inline-flex items-center justify-center gap-1.5 rounded-md bg-red-600 px-3.5 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-200'
            : 'btn-primary'
        }
      >
        {confirmLabel}
      </button>
    </div>
  </Modal>
);

export default ConfirmDialog;
