import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import type { TargetGroup } from "../types/targetGroup";
import { parseCoordinates, invalidCoordinates } from "../types/targetGroup";
import type { TargetTemplate, TemplateWave } from "../types/targetTemplate";
import {
  ALL_COMMAND_TYPES,
  commandTypeLabels,
  createEmptyWave,
} from "../types/targetTemplate";
import { targetGroupService } from "../services/targetGroupService";
import { targetTemplateService } from "../services/targetTemplateService";
import styles from "./TargetGroupsManager.module.css";

// ─── helpers ────────────────────────────────────────────────────────────────

const toTimeInput = (t: string) => t.slice(0, 5);
const toTimeApi = (t: string) => (t.length === 5 ? `${t}:00` : t);

function extractError(err: unknown): string {
  if (err instanceof AxiosError && err.response?.data?.error) {
    return String(err.response.data.error);
  }
  if (err instanceof Error) return err.message;
  return "Wystąpił nieoczekiwany błąd.";
}

function totalCommands(waves: TemplateWave[]): number {
  return waves.reduce((s, w) => s + w.commandNumber, 0);
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

// ─── Wave Editor (self-contained) ────────────────────────────────────────────

interface WaveEditorProps {
  waves: TemplateWave[];
  onChange: (waves: TemplateWave[]) => void;
}

const WaveEditor = ({ waves, onChange }: WaveEditorProps) => {
  const update = (index: number, field: keyof TemplateWave, value: string | number) => {
    onChange(waves.map((w, i) => (i === index ? { ...w, [field]: value } : w)));
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

// ─── Group Detail (expanded card body) ───────────────────────────────────────

interface GroupDetailProps {
  group: TargetGroup;
}

const GroupDetail = ({ group }: GroupDetailProps) => (
  <div className={styles.groupDetail}>
    <div className={styles.detailSection}>
      <h5 className={styles.detailTitle}>Wioski ({group.villageCoordinates.length})</h5>
      <div className={styles.coordChips}>
        {group.villageCoordinates.map((c) => (
          <span key={c} className={styles.coordChip}>{c}</span>
        ))}
      </div>
    </div>

    <div className={styles.detailSection}>
      <h5 className={styles.detailTitle}>
        Fale ataków ({totalCommands(group.waves)} rozkazów)
      </h5>
      <table className={styles.waveTable}>
        <thead>
          <tr>
            <th>Od</th>
            <th>Do</th>
            <th>Ilość</th>
            <th>Typ rozkazu</th>
          </tr>
        </thead>
        <tbody>
          {group.waves.map((w, i) => (
            <tr key={i}>
              <td>{toTimeInput(w.minTime)}</td>
              <td>{toTimeInput(w.maxTime)}</td>
              <td>{w.commandNumber}</td>
              <td>
                <span
                  className={styles.commandBadge}
                  style={{
                    backgroundColor:
                      (commandTypeColor[w.commandType] ?? "#6b7280") + "22",
                    color: commandTypeColor[w.commandType] ?? "#6b7280",
                    borderColor: commandTypeColor[w.commandType] ?? "#6b7280",
                  }}
                >
                  {commandTypeLabels[w.commandType] ?? w.commandType}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  </div>
);

// ─── Group Card ───────────────────────────────────────────────────────────────

interface GroupCardProps {
  group: TargetGroup;
  onEdit: () => void;
  onDelete: () => void;
}

const GroupCard = ({ group, onEdit, onDelete }: GroupCardProps) => {
  const [expanded, setExpanded] = useState(false);
  const cmds = totalCommands(group.waves);

  return (
    <div className={styles.groupCard}>
      <div className={styles.groupCardHeader}>
        <div className={styles.groupCardMeta}>
          <span className={styles.groupName}>{group.name}</span>
          <div className={styles.groupBadges}>
            <span className={styles.badge}>{group.villageCoordinates.length} wiosek</span>
            <span className={styles.badge}>{cmds} rozkazów</span>
            {group.baseTemplateName && (
              <span className={`${styles.badge} ${styles.badgeTemplate}`}>
                📋 {group.baseTemplateName}
              </span>
            )}
          </div>
        </div>
        <div className={styles.groupCardActions}>
          <button
            className={styles.btnExpand}
            onClick={() => setExpanded((v) => !v)}
            aria-label={expanded ? "Ukryj szczegóły" : "Pokaż szczegóły"}
          >
            {expanded ? "▲ Ukryj" : "▼ Szczegóły"}
          </button>
          <button className={styles.btnEdit} onClick={onEdit}>
            Edytuj
          </button>
          <button className={styles.btnDelete} onClick={onDelete}>
            Usuń
          </button>
        </div>
      </div>

      {expanded && <GroupDetail group={group} />}
    </div>
  );
};

// ─── Delete Dialog ────────────────────────────────────────────────────────────

interface DeleteDialogProps {
  groupName: string;
  isDeleting: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

const DeleteDialog = ({ groupName, isDeleting, onConfirm, onCancel }: DeleteDialogProps) => (
  <div className={styles.modalOverlay} role="dialog" aria-modal="true">
    <div className={styles.deleteDialog}>
      <h3>Usuń grupę celi</h3>
      <p>
        Czy na pewno chcesz usunąć grupę <strong>{groupName}</strong>? Tej
        operacji nie można cofnąć.
      </p>
      <div className={styles.dialogActions}>
        <button className={styles.btnSecondary} onClick={onCancel} disabled={isDeleting}>
          Anuluj
        </button>
        <button className={styles.btnDanger} onClick={onConfirm} disabled={isDeleting}>
          {isDeleting ? "Usuwanie…" : "Usuń grupę"}
        </button>
      </div>
    </div>
  </div>
);

// ─── Group Form Modal ─────────────────────────────────────────────────────────

interface GroupFormModalProps {
  scheduleId: string;
  editingGroup: TargetGroup | null;
  templates: TargetTemplate[];
  onSaved: (group: TargetGroup) => void;
  onClose: () => void;
}

const GroupFormModal = ({
  scheduleId,
  editingGroup,
  templates,
  onSaved,
  onClose,
}: GroupFormModalProps) => {
  const isEdit = editingGroup !== null;

  const [name, setName] = useState(editingGroup?.name ?? "");
  const [rawCoords, setRawCoords] = useState(
    editingGroup ? editingGroup.villageCoordinates.join(" ") : "",
  );
  const [selectedTemplateId, setSelectedTemplateId] = useState(
    editingGroup?.baseTemplateId ?? "",
  );
  const [waves, setWaves] = useState<TemplateWave[]>(
    editingGroup ? [...editingGroup.waves] : [],
  );
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  const parsedCoords = parseCoordinates(rawCoords);
  const badTokens = invalidCoordinates(rawCoords);

  const defaultTemplates = templates.filter((t) => t.isDefault);
  const userTemplates = templates.filter((t) => !t.isDefault);

  const handleTemplateChange = (templateId: string) => {
    setSelectedTemplateId(templateId);
    if (templateId) {
      const tpl = templates.find((t) => t.id === templateId);
      if (tpl) {
        setWaves([...tpl.waves]);
      }
    }
  };

  const selectedTemplate = templates.find((t) => t.id === selectedTemplateId);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (parsedCoords.length === 0) {
      setSaveError("Podaj przynajmniej jedną poprawną koordynatę wioski.");
      return;
    }
    if (waves.length === 0) {
      setSaveError("Dodaj przynajmniej jedną falę ataków.");
      return;
    }

    setSaveError(null);
    setIsSaving(true);

    try {
      const payload = {
        name: name.trim(),
        villageCoordinates: parsedCoords,
        waves,
        baseTemplateId: selectedTemplateId || null,
        baseTemplateName: selectedTemplate?.name ?? null,
      };

      let saved: TargetGroup;
      if (isEdit) {
        saved = await targetGroupService.updateGroup(
          scheduleId,
          editingGroup!.id,
          payload,
        );
      } else {
        saved = await targetGroupService.createGroup(scheduleId, payload);
      }

      onSaved(saved);
    } catch (err) {
      setSaveError(extractError(err));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className={styles.modalOverlay} role="dialog" aria-modal="true">
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <h3>{isEdit ? "Edytuj grupę celi" : "Nowa grupa celi"}</h3>
          <button
            className={styles.modalClose}
            onClick={onClose}
            aria-label="Zamknij"
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className={styles.modalBody}>
          {/* Name */}
          <div className={styles.fieldGroup}>
            <label htmlFor="grp-name" className={styles.fieldLabel}>
              Nazwa grupy
            </label>
            <input
              id="grp-name"
              type="text"
              className={styles.fieldInput}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="np. Grimnar – fala 1"
              maxLength={200}
              required
            />
          </div>

          {/* Village coordinates */}
          <div className={styles.fieldGroup}>
            <label htmlFor="grp-coords" className={styles.fieldLabel}>
              Koordynaty wiosek wroga
            </label>
            <textarea
              id="grp-coords"
              className={styles.coordsTextarea}
              value={rawCoords}
              onChange={(e) => setRawCoords(e.target.value)}
              placeholder={"Wklej koordynaty oddzielone spacją lub nową linią\nnp. 473|490 472|488 475|489"}
              rows={4}
            />
            <div className={styles.coordsFeedback}>
              {parsedCoords.length > 0 && (
                <span className={styles.coordsValid}>
                  ✓ Rozpoznano {parsedCoords.length}{" "}
                  {parsedCoords.length === 1 ? "wioskę" : parsedCoords.length < 5 ? "wioski" : "wiosek"}
                </span>
              )}
              {badTokens.length > 0 && (
                <span className={styles.coordsInvalid}>
                  ⚠ Nieprawidłowe wpisy ({badTokens.length}): {badTokens.slice(0, 3).join(", ")}
                  {badTokens.length > 3 ? "…" : ""}
                </span>
              )}
              {rawCoords.trim().length === 0 && (
                <span className={styles.coordsHint}>
                  Oczekiwany format: X|Y (np. 473|490)
                </span>
              )}
            </div>
          </div>

          {/* Template Selector */}
          <div className={styles.fieldGroup}>
            <label htmlFor="grp-template" className={styles.fieldLabel}>
              Szablon fal ataków
            </label>
            <div className={styles.templateSelectorRow}>
              <select
                id="grp-template"
                className={styles.fieldSelect}
                value={selectedTemplateId}
                onChange={(e) => handleTemplateChange(e.target.value)}
              >
                <option value="">— Własne fale (bez szablonu) —</option>
                {defaultTemplates.length > 0 && (
                  <optgroup label="Domyślne szablony">
                    {defaultTemplates.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))}
                  </optgroup>
                )}
                {userTemplates.length > 0 && (
                  <optgroup label="Moje szablony">
                    {userTemplates.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))}
                  </optgroup>
                )}
              </select>
            </div>
            {selectedTemplateId && (
              <p className={styles.templateNote}>
                ℹ Fale zostały załadowane z szablonu. Możesz je dowolnie
                modyfikować — zmiany zapisują się razem z grupą, nie wpływają
                na oryginalny szablon.
              </p>
            )}
          </div>

          {/* Wave Editor */}
          <WaveEditor waves={waves} onChange={setWaves} />

          {saveError && (
            <div className={styles.saveError}>{saveError}</div>
          )}

          <div className={styles.modalFooter}>
            <button
              type="button"
              className={styles.btnSecondary}
              onClick={onClose}
              disabled={isSaving}
            >
              Anuluj
            </button>
            <button
              type="submit"
              className={styles.btnPrimary}
              disabled={isSaving}
            >
              {isSaving
                ? "Zapisywanie…"
                : isEdit
                ? "Zapisz zmiany"
                : "Utwórz grupę"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// ─── Main Component ───────────────────────────────────────────────────────────

interface TargetGroupsManagerProps {
  scheduleId: string;
}

export const TargetGroupsManager = ({ scheduleId }: TargetGroupsManagerProps) => {
  const [groups, setGroups] = useState<TargetGroup[]>([]);
  const [templates, setTemplates] = useState<TargetTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingGroup, setEditingGroup] = useState<TargetGroup | null>(null);
  const [deletingGroup, setDeletingGroup] = useState<TargetGroup | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [grps, tpls] = await Promise.all([
        targetGroupService.getGroups(scheduleId),
        targetTemplateService.getTemplates(),
      ]);
      setGroups(grps);
      setTemplates(tpls);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setIsLoading(false);
    }
  }, [scheduleId]);

  useEffect(() => {
    load();
  }, [load]);

  const handleSaved = (saved: TargetGroup) => {
    setGroups((prev) => {
      const existing = prev.find((g) => g.id === saved.id);
      if (existing) {
        return prev.map((g) => (g.id === saved.id ? saved : g));
      }
      return [...prev, saved].sort((a, b) => a.name.localeCompare(b.name));
    });
    setShowForm(false);
    setEditingGroup(null);
  };

  const handleEditClick = (group: TargetGroup) => {
    setEditingGroup(group);
    setShowForm(true);
  };

  const handleDeleteClick = (group: TargetGroup) => {
    setDeletingGroup(group);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingGroup) return;
    setIsDeleting(true);
    try {
      await targetGroupService.deleteGroup(scheduleId, deletingGroup.id);
      setGroups((prev) => prev.filter((g) => g.id !== deletingGroup.id));
      setDeletingGroup(null);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingGroup(null);
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div>
          <h3 className={styles.title}>Grupy celi</h3>
          <p className={styles.subtitle}>
            Każda grupa to lista wiosek wroga, które zostaną uderzone tymi
            samymi falami ataków.
          </p>
        </div>
        <button
          className={styles.btnPrimary}
          onClick={() => {
            setEditingGroup(null);
            setShowForm(true);
          }}
        >
          + Dodaj grupę
        </button>
      </div>

      {error && (
        <div className={styles.errorBanner}>
          {error}
          <button className={styles.retryBtn} onClick={load}>
            Spróbuj ponownie
          </button>
        </div>
      )}

      {isLoading ? (
        <div className={styles.loadingState}>Ładowanie grup celi…</div>
      ) : groups.length === 0 ? (
        <div className={styles.emptyState}>
          <p className={styles.emptyTitle}>Brak zdefiniowanych grup celi</p>
          <p className={styles.emptyText}>
            Utwórz pierwszą grupę, aby przypisać wioski wroga i fale ataków do
            tej rozpiski.
          </p>
          <button
            className={styles.btnPrimary}
            onClick={() => {
              setEditingGroup(null);
              setShowForm(true);
            }}
          >
            + Utwórz pierwszą grupę
          </button>
        </div>
      ) : (
        <div className={styles.groupList}>
          {groups.map((group) => (
            <GroupCard
              key={group.id}
              group={group}
              onEdit={() => handleEditClick(group)}
              onDelete={() => handleDeleteClick(group)}
            />
          ))}
        </div>
      )}

      {showForm && (
        <GroupFormModal
          scheduleId={scheduleId}
          editingGroup={editingGroup}
          templates={templates}
          onSaved={handleSaved}
          onClose={handleCloseForm}
        />
      )}

      {deletingGroup && (
        <DeleteDialog
          groupName={deletingGroup.name}
          isDeleting={isDeleting}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setDeletingGroup(null)}
        />
      )}
    </div>
  );
};
