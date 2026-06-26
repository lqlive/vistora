import React, { useEffect, useState } from 'react';
import Modal from '../../shared/components/Modal';
import type { DatasetItem, DatasetRequest } from '../../types';

interface DatasetDialogProps {
  open: boolean;
  dataset: DatasetItem | null;
  saving: boolean;
  onCancel: () => void;
  onConfirm: (request: DatasetRequest) => void;
}

const emptyRequest: DatasetRequest = {
  name: '',
  sql: 'select 1',
  description: null,
};

const DatasetDialog: React.FC<DatasetDialogProps> = ({
  open,
  dataset,
  saving,
  onCancel,
  onConfirm,
}) => {
  const [request, setRequest] = useState<DatasetRequest>(emptyRequest);

  useEffect(() => {
    if (!open) return;

    setRequest(
      dataset
        ? {
            name: dataset.name,
            sql: dataset.sql,
            description: dataset.description ?? null,
          }
        : emptyRequest
    );
  }, [dataset, open]);

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    const name = request.name.trim();
    const sql = request.sql.trim();
    if (!name || !sql || saving) return;

    onConfirm({
      name,
      sql,
      description: request.description?.trim() || null,
    });
  };

  return (
    <Modal open={open} title={dataset ? 'Edit dataset' : 'New dataset'} onClose={onCancel}>
      <form onSubmit={handleSubmit}>
        <div className="space-y-4 px-5 py-4">
          <label className="block text-sm font-medium text-gray-700">
            Name
            <input
              autoFocus
              value={request.name}
              onChange={(event) => setRequest((current) => ({ ...current, name: event.target.value }))}
              className="input mt-2"
              placeholder="Dataset name"
            />
          </label>

          <label className="block text-sm font-medium text-gray-700">
            SQL
            <textarea
              value={request.sql}
              onChange={(event) => setRequest((current) => ({ ...current, sql: event.target.value }))}
              className="input mt-2 min-h-[160px] font-mono text-sm"
              placeholder="select * from ..."
            />
          </label>

          <label className="block text-sm font-medium text-gray-700">
            Description
            <textarea
              value={request.description ?? ''}
              onChange={(event) =>
                setRequest((current) => ({ ...current, description: event.target.value }))
              }
              className="input mt-2 min-h-[80px]"
              placeholder="Optional description"
            />
          </label>
        </div>

        <div className="flex justify-end gap-2 border-t border-gray-100 px-5 py-4">
          <button type="button" onClick={onCancel} className="btn-secondary" disabled={saving}>
            Cancel
          </button>
          <button
            type="submit"
            disabled={saving || !request.name.trim() || !request.sql.trim()}
            className="btn-primary"
          >
            {saving ? 'Saving...' : 'Save'}
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default DatasetDialog;
