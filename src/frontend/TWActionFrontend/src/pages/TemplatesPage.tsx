import { useState, useEffect, useCallback } from "react";
import { useAuth } from "../hooks/useAuth";
import { targetTemplateService } from "../services/targetTemplateService";
import type { TargetTemplate, TemplateWave } from "../types/targetTemplate";
import {
  ALL_COMMAND_TYPES,
  commandTypeLabels,
  createEmptyWave,
} from "../types/targetTemplate";
import { AxiosError } from "axios";
import styles from "./TemplatesPage.module.css";

// ─── helpers ────────────────────────────────────────────────────────────────

/** Convert "HH:mm:ss" (backend) → "HH:mm" (HTML time input). */
const toTimeInput = (t: string) => t.slice(0, 5);

/** Convert "HH:mm" (HTML time input) → "HH:mm:ss" (backend). */
const toTimeApi = (t: string) => (t.length === 5 ? `${t}:00` : t);

function extractError(err: unknown): string {
  if (err instanceof AxiosError && err.response?.data?.error) {
    return String(err.response.data.error);
  }
  if (err instanceof Error) return err.message;
  return "Wystąpił nieoczekiwany błąd.";
}

const commandTypeColor: Record<string, string> = {
  Off: "#16a34a",
  FakeOffensive: "#d97706",
  FakeDefensive: "#b45309",
  Catapults: "#7c3aed",
  NobleWithDeff: "#0369a1",
  NobleWithFullOff: "#0369a1",
  NobleWithHalfOff: "#0369a1",
  NobleWithQuarterOffensive: "#0369a1",
  NobleWith150Axes: "#0369a1",
  NobleWith100HeavyCavalry: "#0369a1",
  RandomNoble: "#0369a1",
};

// ─── Wave Editor ─────────────────────────────────────────────────────────────

interface WaveEditorProps {
  waves: TemplateWave[];
  onChange: (waves: TemplateWave[]) => void;
}

const WaveEditor = ({ waves, onChange }: WaveEditorProps) => {
  const update = (
    index: number,
    field: keyof TemplateWave,
    value: string | number,
  ) => {
    const next = waves.map((w, i) =>
      i === index ? { ...w, [field]: value } : w,
    );
    onChange(next);
  };

  const remove = (index: number) => {
    onChange(waves.filter((_, i) => i !== index));
  };

  const add = () => {
    const last = waves[waves.length - 1];
    onChange([...waves, last ? { ...last } : createEmptyWave()]);
  };

  return (
    <div className={styles.waveEditor}>
      <div className={styles.waveEditorHeader}>
        <span>Fale ataków</span>
        <button type="button" className={styles.addWaveBtn} onClick={add}>
          + Dodaj falę
        </button>
      </div>
      {waves.length === 0 && (
        <p className={styles.emptyWaves}>Brak fal. Dodaj przynajmniej jedną.</p>
      )}
      {waves.map((wave, i) => (
        <div key={i} className={styles.waveRow}>
          <div className={styles.waveField}>
            <label>Od</label>
            <input
              type="time"
              value={toTimeInput(wave.minTime)}
              onChange={(e) => update(i, "minTime", toTimeApi(e.target.value))}
              required
            />
          </div>
          <div className={styles.waveField}>
            <label>Do</label>
            <input
              type="time"
              value={toTimeInput(wave.maxTime)}
              onChange={(e) => update(i, "maxTime", toTimeApi(e.target.value))}
              required
            />
          </div>
          <div className={styles.waveField}>
            <label>Ilość</label>
            <input
              type="number"
              min={1}
              max={99}
              value={wave.commandNumber}
              onChange={(e) =>
                update(i, "commandNumber", parseInt(e.target.value, 10) || 1)
              }
              required
            />
          </div>
          <div className={`${styles.waveField} ${styles.waveTypeField}`}>
            <label>Typ rozkazu</label>
            <select
              value={wave.commandType}
              onChange={(e) => update(i, "commandType", e.target.value)}
            >
              {ALL_COMMAND_TYPES.map((ct) => (
                <option key={ct} value={ct}>
                  {commandTypeLabels[ct]}
                </option>
              ))}
            </select>
          </div>
          <button
            type="button"
            className={styles.removeWaveBtn}
            onClick={() => remove(i)}
            aria-label="Usuń falę"
            title="Usuń falę"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
};

// ─── Template Form Modal ──────────────────────────────────────────────────────

interface TemplateFormProps {
  template?: TargetTemplate;
  onSave: (name: string, waves: TemplateWave[]) => Promise<void>;
  onClose: () => void;
  isSaving: boolean;
  saveError: string | null;
}

const TemplateFormModal = ({
  template,
  onSave,
  onClose,
  isSaving,
  saveError,
}: TemplateFormProps) => {
  const [name, setName] = useState(template?.name ?? "");
  const [waves, setWaves] = useState<TemplateWave[]>(
    template?.waves.length ? template.waves : [createEmptyWave()],
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onSave(name.trim(), waves);
  };

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <div
        className={styles.modal}
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
      >
        <div className={styles.modalHeader}>
          <h2 id="modal-title">
            {template ? "Edytuj szablon" : "Nowy szablon"}
          </h2>
          <button
            type="button"
            className={styles.closeBtn}
            onClick={onClose}
            aria-label="Zamknij formularz"
          >
            ×
          </button>
        </div>

        {saveError && <div className={styles.error}>{saveError}</div>}

        <form onSubmit={handleSubmit}>
          <div className={styles.formGroup}>
            <label htmlFor="template-name">Nazwa szablonu</label>
            <input
              id="template-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="np. 20 OFF Premium"
              maxLength={200}
              required
              autoFocus
            />
          </div>

          <WaveEditor waves={waves} onChange={setWaves} />

          <div className={styles.modalActions}>
            <button
              type="button"
              className={styles.cancelBtn}
              onClick={onClose}
              disabled={isSaving}
            >
              Anuluj
            </button>
            <button
              type="submit"
              className={styles.submitBtn}
              disabled={isSaving || waves.length === 0}
            >
              {isSaving ? "Zapisywanie..." : "Zapisz szablon"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// ─── Template Card ────────────────────────────────────────────────────────────

interface TemplateCardProps {
  template: TargetTemplate;
  onEdit?: (template: TargetTemplate) => void;
  onDelete?: (template: TargetTemplate) => void;
}

const TemplateCard = ({ template, onEdit, onDelete }: TemplateCardProps) => {
  const [expanded, setExpanded] = useState(false);

  return (
    <div
      className={`${styles.card} ${template.isDefault ? styles.defaultCard : styles.userCard}`}
    >
      <div className={styles.cardHeader}>
        <div className={styles.cardTitle}>
          <span className={styles.templateName}>{template.name}</span>
          {template.isDefault && (
            <span className={styles.defaultBadge}>Domyślny</span>
          )}
        </div>
        <div className={styles.cardActions}>
          <button
            type="button"
            className={styles.expandBtn}
            onClick={() => setExpanded((v) => !v)}
            aria-expanded={expanded}
          >
            {expanded
              ? "Zwiń ▲"
              : `${template.waves.reduce((s, w) => s + w.commandNumber, 0)} rozkazów ▼`}
          </button>
          {!template.isDefault && onEdit && (
            <button
              type="button"
              className={styles.editBtn}
              onClick={() => onEdit(template)}
            >
              Edytuj
            </button>
          )}
          {!template.isDefault && onDelete && (
            <button
              type="button"
              className={styles.deleteBtn}
              onClick={() => onDelete(template)}
            >
              Usuń
            </button>
          )}
        </div>
      </div>

      {expanded && (
        <div className={styles.waveTable}>
          <table>
            <thead>
              <tr>
                <th>#</th>
                <th>Okno czasu</th>
                <th>Ilość</th>
                <th>Typ rozkazu</th>
              </tr>
            </thead>
            <tbody>
              {template.waves.map((wave, i) => (
                <tr key={i}>
                  <td>{i + 1}</td>
                  <td>
                    {toTimeInput(wave.minTime)} – {toTimeInput(wave.maxTime)}
                  </td>
                  <td>{wave.commandNumber}×</td>
                  <td>
                    <span
                      className={styles.commandTypeBadge}
                      style={{
                        backgroundColor: `${commandTypeColor[wave.commandType] ?? "#64748b"}22`,
                        color: commandTypeColor[wave.commandType] ?? "#64748b",
                        borderColor: `${commandTypeColor[wave.commandType] ?? "#64748b"}44`,
                      }}
                    >
                      {commandTypeLabels[wave.commandType] ?? wave.commandType}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

// ─── Delete Confirmation Dialog ───────────────────────────────────────────────

interface DeleteDialogProps {
  template: TargetTemplate;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

const DeleteDialog = ({
  template,
  onConfirm,
  onCancel,
  isDeleting,
}: DeleteDialogProps) => (
  <div className={styles.modalOverlay} onClick={onCancel}>
    <div
      className={`${styles.modal} ${styles.confirmModal}`}
      onClick={(e) => e.stopPropagation()}
      role="alertdialog"
      aria-labelledby="confirm-title"
    >
      <h2 id="confirm-title">Usuń szablon</h2>
      <p>
        Czy na pewno chcesz usunąć szablon{" "}
        <strong>&ldquo;{template.name}&rdquo;</strong>? Tej operacji nie można
        cofnąć.
      </p>
      <div className={styles.modalActions}>
        <button
          type="button"
          className={styles.cancelBtn}
          onClick={onCancel}
          disabled={isDeleting}
        >
          Anuluj
        </button>
        <button
          type="button"
          className={styles.dangerBtn}
          onClick={onConfirm}
          disabled={isDeleting}
        >
          {isDeleting ? "Usuwanie..." : "Usuń"}
        </button>
      </div>
    </div>
  </div>
);

// ─── Main Page ────────────────────────────────────────────────────────────────

const TemplatesPage = () => {
  const { user, isAuthenticated, isLoading: authLoading } = useAuth();
  const [templates, setTemplates] = useState<TargetTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal state
  const [showForm, setShowForm] = useState(false);
  const [editingTemplate, setEditingTemplate] = useState<
    TargetTemplate | undefined
  >();
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  // Delete confirmation state
  const [deletingTemplate, setDeletingTemplate] = useState<
    TargetTemplate | undefined
  >();
  const [isDeleting, setIsDeleting] = useState(false);

  const loadTemplates = useCallback(async () => {
    if (!isAuthenticated) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await targetTemplateService.getTemplates();
      setTemplates(data);
    } catch (err) {
      setError("Nie udało się załadować szablonów.");
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    loadTemplates();
  }, [loadTemplates]);

  const handleOpenCreate = () => {
    setEditingTemplate(undefined);
    setSaveError(null);
    setShowForm(true);
  };

  const handleOpenEdit = (template: TargetTemplate) => {
    setEditingTemplate(template);
    setSaveError(null);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingTemplate(undefined);
    setSaveError(null);
  };

  const handleSave = async (name: string, waves: TemplateWave[]) => {
    setIsSaving(true);
    setSaveError(null);
    try {
      if (editingTemplate) {
        const updated = await targetTemplateService.updateTemplate(
          editingTemplate.id,
          {
            name,
            waves,
          },
        );
        setTemplates((prev) =>
          prev.map((t) => (t.id === updated.id ? updated : t)),
        );
      } else {
        const created = await targetTemplateService.createTemplate({
          name,
          waves,
        });
        setTemplates((prev) => [...prev, created]);
      }
      setShowForm(false);
      setEditingTemplate(undefined);
    } catch (err: unknown) {
      setSaveError(extractError(err));
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteRequest = (template: TargetTemplate) => {
    setDeletingTemplate(template);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingTemplate) return;
    setIsDeleting(true);
    try {
      await targetTemplateService.deleteTemplate(deletingTemplate.id);
      setTemplates((prev) => prev.filter((t) => t.id !== deletingTemplate.id));
      setDeletingTemplate(undefined);
    } catch (err) {
      setError(extractError(err));
      setDeletingTemplate(undefined);
    } finally {
      setIsDeleting(false);
    }
  };

  const defaultTemplates = templates.filter((t) => t.isDefault);
  const userTemplates = templates.filter((t) => !t.isDefault);

  if (authLoading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>Ładowanie...</div>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return (
      <div className={styles.container}>
        <div className={styles.loginCard}>
          <p>Zaloguj się aby zarządzać szablonami akcji.</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <header className={styles.pageHeader}>
        <div>
          <h1 className={styles.title}>🎯 Szablony akcji</h1>
          <p className={styles.subtitle}>
            Zarządzaj szablonami rozkazów używanymi przy generowaniu akcji
          </p>
        </div>
        <button
          type="button"
          className={styles.newBtn}
          onClick={handleOpenCreate}
        >
          + Nowy szablon
        </button>
      </header>

      {error && (
        <div className={styles.error} role="alert">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className={styles.loading}>Ładowanie szablonów...</div>
      ) : (
        <>
          {/* Default templates section */}
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>
              📦 Szablony domyślne{" "}
              <span className={styles.count}>({defaultTemplates.length})</span>
            </h2>
            <p className={styles.sectionHint}>
              Szablony bazowe dostępne dla wszystkich użytkowników. Są tylko do
              odczytu i nie można ich usunąć.
            </p>
            {defaultTemplates.length === 0 ? (
              <p className={styles.emptyState}>Brak szablonów domyślnych.</p>
            ) : (
              <div className={styles.cardGrid}>
                {defaultTemplates.map((t) => (
                  <TemplateCard key={t.id} template={t} />
                ))}
              </div>
            )}
          </section>

          {/* User templates section */}
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>
              👤 Moje szablony{" "}
              <span className={styles.count}>({userTemplates.length})</span>
            </h2>
            {userTemplates.length === 0 ? (
              <div className={styles.emptyState}>
                <p>Nie masz jeszcze własnych szablonów.</p>
                <button
                  type="button"
                  className={styles.newBtn}
                  onClick={handleOpenCreate}
                >
                  Utwórz pierwszy szablon
                </button>
              </div>
            ) : (
              <div className={styles.cardGrid}>
                {userTemplates.map((t) => (
                  <TemplateCard
                    key={t.id}
                    template={t}
                    onEdit={handleOpenEdit}
                    onDelete={handleDeleteRequest}
                  />
                ))}
              </div>
            )}
          </section>
        </>
      )}

      {/* Create / Edit modal */}
      {showForm && (
        <TemplateFormModal
          template={editingTemplate}
          onSave={handleSave}
          onClose={handleCloseForm}
          isSaving={isSaving}
          saveError={saveError}
        />
      )}

      {/* Delete confirmation */}
      {deletingTemplate && (
        <DeleteDialog
          template={deletingTemplate}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setDeletingTemplate(undefined)}
          isDeleting={isDeleting}
        />
      )}
    </div>
  );
};

export default TemplatesPage;
