import { useCallback, useEffect, useMemo, useState } from "react";
import type {
  UpdateUserRequest,
  User,
  UserRole,
  UserSession,
} from "../types/user";
import { userService } from "../services/userService";
import { useI18n } from "../i18n/useI18n";
import styles from "./UserPanel.module.css";

type RoleFilter = "all" | UserRole;
type SortField = "email" | "displayName" | "provider" | "role" | "createdAt";
type SortDirection = "asc" | "desc";

const PAGE_SIZE_OPTIONS = [10, 25, 50];

const UserPanel = () => {
  const { t, language } = useI18n();
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [operationError, setOperationError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState<RoleFilter>("all");
  const [sortField, setSortField] = useState<SortField>("createdAt");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [sessions, setSessions] = useState<UserSession[]>([]);
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);
  const [revokingSessionId, setRevokingSessionId] = useState<string | null>(null);
  const [isRevokingAll, setIsRevokingAll] = useState(false);

  const loadUsers = useCallback(
    async (manualRefresh = false) => {
      try {
        if (manualRefresh) setIsRefreshing(true);
        else setIsLoading(true);
        setError(null);
        setOperationError(null);
        setUsers(await userService.getAllUsers());
      } catch (err) {
        console.error("Error loading users:", err);
        setError(t.userPanel.error);
      } finally {
        setIsLoading(false);
        setIsRefreshing(false);
      }
    },
    [t.userPanel.error],
  );

  useEffect(() => {
    void loadUsers();
  }, [loadUsers]);

  const filteredUsers = useMemo(() => {
    const query = search.trim().toLocaleLowerCase();
    const filtered = users.filter((user) => {
      const matchesSearch = !query || [user.email, user.displayName ?? "", user.provider]
        .some((value) => value.toLocaleLowerCase().includes(query));
      const matchesRole = roleFilter === "all" || user.role === roleFilter;
      return matchesSearch && matchesRole;
    });

    return [...filtered].sort((first, second) => {
      const firstValue = sortField === "createdAt"
        ? new Date(first.createdAt).getTime()
        : (first[sortField] ?? "").toString().toLocaleLowerCase();
      const secondValue = sortField === "createdAt"
        ? new Date(second.createdAt).getTime()
        : (second[sortField] ?? "").toString().toLocaleLowerCase();
      if (firstValue < secondValue) return sortDirection === "asc" ? -1 : 1;
      if (firstValue > secondValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });
  }, [roleFilter, search, sortDirection, sortField, users]);

  const totalPages = Math.max(1, Math.ceil(filteredUsers.length / pageSize));
  const visibleUsers = filteredUsers.slice((page - 1) * pageSize, page * pageSize);

  useEffect(() => {
    setPage((currentPage) => Math.min(currentPage, totalPages));
  }, [totalPages]);

  const formatDate = useCallback(
    (date: string) => new Date(date).toLocaleDateString(
      language === "pl" ? "pl-PL" : "en-US",
      { year: "numeric", month: "short", day: "numeric" },
    ),
    [language],
  );

  const formatDateTime = useCallback(
    (date: string) => new Date(date).toLocaleString(
      language === "pl" ? "pl-PL" : "en-US",
      { dateStyle: "medium", timeStyle: "short" },
    ),
    [language],
  );

  const roleLabel = useCallback(
    (role: string) => role === "Admin" ? t.userPanel.adminRole : t.userPanel.userRole,
    [t.userPanel.adminRole, t.userPanel.userRole],
  );

  const handleSort = (field: SortField) => {
    if (sortField === field) setSortDirection((current) => current === "asc" ? "desc" : "asc");
    else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const handleOpenDetails = async (user: User) => {
    setSelectedUser(user);
    setOperationError(null);
    setIsLoadingDetails(true);
    try {
      const [freshUser, userSessions] = await Promise.all([
        userService.getUser(user.id),
        userService.getUserSessions(user.id),
      ]);
      setSelectedUser(freshUser);
      setUsers((current) => current.map((item) => item.id === freshUser.id ? freshUser : item));
      setSessions(userSessions);
    } catch (err) {
      console.error("Error loading user details:", err);
      setOperationError(t.userPanel.operationError);
    } finally {
      setIsLoadingDetails(false);
    }
  };

  const handleSave = async (request: UpdateUserRequest) => {
    if (!editingUser) return;
    setIsSaving(true);
    setOperationError(null);
    try {
      const updatedUser = await userService.updateUser(editingUser.id, request);
      setUsers((current) => current.map((item) => item.id === updatedUser.id ? updatedUser : item));
      setSelectedUser((current) => current?.id === updatedUser.id ? updatedUser : current);
      setEditingUser(null);
    } catch (err) {
      console.error("Error updating user:", err);
      setOperationError(t.userPanel.operationError);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (user: User) => {
    if (!window.confirm(t.userPanel.confirmDelete)) return;
    setDeletingUserId(user.id);
    setOperationError(null);
    try {
      await userService.deleteUser(user.id);
      setUsers((current) => current.filter((item) => item.id !== user.id));
      setSelectedUser((current) => current?.id === user.id ? null : current);
      setEditingUser((current) => current?.id === user.id ? null : current);
    } catch (err) {
      console.error("Error deleting user:", err);
      setOperationError(t.userPanel.operationError);
    } finally {
      setDeletingUserId(null);
    }
  };

  const refreshSessions = async (userId: string) => {
    try {
      setSessions(await userService.getUserSessions(userId));
    } catch (err) {
      console.error("Error loading user sessions:", err);
      setOperationError(t.userPanel.operationError);
    }
  };

  const handleRevokeSession = async (session: UserSession) => {
    if (!selectedUser) return;
    setRevokingSessionId(session.id);
    setOperationError(null);
    try {
      await userService.deleteSession(selectedUser.id, session.id);
      await refreshSessions(selectedUser.id);
    } catch (err) {
      console.error("Error revoking session:", err);
      setOperationError(t.userPanel.operationError);
    } finally {
      setRevokingSessionId(null);
    }
  };

  const handleRevokeAllSessions = async () => {
    if (!selectedUser || !window.confirm(t.userPanel.confirmRevokeAll)) return;
    setIsRevokingAll(true);
    setOperationError(null);
    try {
      await userService.deleteAllSessions(selectedUser.id);
      await refreshSessions(selectedUser.id);
    } catch (err) {
      console.error("Error revoking sessions:", err);
      setOperationError(t.userPanel.operationError);
    } finally {
      setIsRevokingAll(false);
    }
  };

  const sortIndicator = (field: SortField) => sortField === field
    ? (sortDirection === "asc" ? " ↑" : " ↓")
    : "";

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div>
          <h1>{t.userPanel.title}</h1>
          <span className={styles.badge}>{users.length} {t.userPanel.usersCount}</span>
        </div>
        <button type="button" className={styles.refreshButton} onClick={() => void loadUsers(true)} disabled={isRefreshing || isLoading}>
          ↻ {isRefreshing ? t.userPanel.refreshing : t.userPanel.refresh}
        </button>
      </header>

      {error && <div className={styles.error} role="alert"><span>{error}</span><button type="button" onClick={() => void loadUsers(true)} disabled={isRefreshing}>{t.userPanel.retry}</button></div>}
      {operationError && <div className={styles.error} role="alert">{operationError}</div>}

      {isLoading ? <div className={styles.loading}>{t.userPanel.loading}</div> : (
        <>
          <section className={styles.toolbar} aria-label={t.userPanel.title}>
            <input type="search" value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder={t.userPanel.searchPlaceholder} className={styles.searchInput} />
            <label className={styles.filterLabel}>{t.userPanel.roleFilter}
              <select value={roleFilter} onChange={(event) => { setRoleFilter(event.target.value as RoleFilter); setPage(1); }}>
                <option value="all">{t.userPanel.allRoles}</option>
                <option value="User">{t.userPanel.userRole}</option>
                <option value="Admin">{t.userPanel.adminRole}</option>
              </select>
            </label>
            <label className={styles.filterLabel}>{t.userPanel.pageSize}
              <select value={pageSize} onChange={(event) => { setPageSize(Number(event.target.value)); setPage(1); }}>
                {PAGE_SIZE_OPTIONS.map((size) => <option key={size} value={size}>{size}</option>)}
              </select>
            </label>
          </section>

          {visibleUsers.length === 0 ? <div className={styles.empty}>{t.userPanel.empty}</div> : (
            <div className={styles.tableWrapper}>
              <table className={styles.table}>
                <thead><tr>
                  {([
                    ["email", t.userPanel.columns.email, t.userPanel.sortEmail],
                    ["displayName", t.userPanel.columns.name, t.userPanel.sortName],
                    ["provider", t.userPanel.columns.provider, t.userPanel.sortProvider],
                    ["role", t.userPanel.columns.role, t.userPanel.sortRole],
                    ["createdAt", t.userPanel.columns.createdAt, t.userPanel.sortCreatedAt],
                  ] as const).map(([field, label, ariaLabel]) => <th key={field}><button type="button" className={styles.sortButton} onClick={() => handleSort(field)} aria-label={ariaLabel}>{label}{sortIndicator(field)}</button></th>)}
                  <th>{t.userPanel.details}</th>
                </tr></thead>
                <tbody>{visibleUsers.map((user) => <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>{user.displayName ?? t.userPanel.noName}</td>
                  <td>{user.provider}</td>
                  <td><span className={`${styles.roleBadge} ${user.role === "Admin" ? styles.roleAdmin : styles.roleUser}`}>{roleLabel(user.role)}</span></td>
                  <td>{formatDate(user.createdAt)}</td>
                  <td className={styles.actions}>
                    <button type="button" className={styles.secondaryButton} onClick={() => void handleOpenDetails(user)}>{t.userPanel.details}</button>
                    <button type="button" className={styles.secondaryButton} onClick={() => setEditingUser(user)}>{t.userPanel.edit}</button>
                    <button type="button" className={styles.dangerButton} onClick={() => void handleDelete(user)} disabled={deletingUserId === user.id}>{deletingUserId === user.id ? t.userPanel.deleting : t.userPanel.delete}</button>
                  </td>
                </tr>)}</tbody>
              </table>
            </div>
          )}

          <div className={styles.pagination}>
            <button type="button" onClick={() => setPage((current) => current - 1)} disabled={page <= 1}>← {t.userPanel.previous}</button>
            <span>{t.userPanel.page} {page} / {totalPages}</span>
            <button type="button" onClick={() => setPage((current) => current + 1)} disabled={page >= totalPages}>{t.userPanel.next} →</button>
          </div>
        </>
      )}

      {selectedUser && <div className={styles.modalOverlay} onClick={() => setSelectedUser(null)}>
        <div className={styles.modal} onClick={(event) => event.stopPropagation()} role="dialog" aria-modal="true">
          <div className={styles.modalHeader}><h2>{t.userPanel.detailsTitle}</h2><button type="button" className={styles.closeButton} onClick={() => setSelectedUser(null)} aria-label={t.userPanel.close}>×</button></div>
          {isLoadingDetails ? <div className={styles.loading}>{t.userPanel.loading}</div> : <>
            <dl className={styles.detailsGrid}>
              <dt>{t.userPanel.emailLabel}</dt><dd>{selectedUser.email}</dd>
              <dt>{t.userPanel.nameLabel}</dt><dd>{selectedUser.displayName ?? t.userPanel.noName}</dd>
              <dt>{t.userPanel.roleLabel}</dt><dd>{roleLabel(selectedUser.role)}</dd>
              <dt>{t.userPanel.providerLabel}</dt><dd>{selectedUser.provider}</dd>
              <dt>{t.userPanel.createdAtLabel}</dt><dd>{formatDateTime(selectedUser.createdAt)}</dd>
            </dl>
            <div className={styles.sessionsHeader}><h3>{t.userPanel.sessionsTitle}</h3><button type="button" className={styles.dangerButton} onClick={() => void handleRevokeAllSessions()} disabled={isRevokingAll || sessions.length === 0}>{isRevokingAll ? t.userPanel.revoking : t.userPanel.revokeAllSessions}</button></div>
            {sessions.length === 0 ? <p className={styles.emptyInline}>{t.userPanel.sessionsEmpty}</p> : <div className={styles.sessionList}>{sessions.map((session) => <div className={styles.sessionRow} key={session.id}>
              <div><code>{session.id}</code><span>{t.userPanel.expiresAt}: {formatDateTime(session.expiresAt)}</span></div>
              <div className={session.isActive ? styles.activeSession : styles.expiredSession}>{session.isActive ? t.userPanel.sessionActive : t.userPanel.sessionExpired}<button type="button" className={styles.dangerButton} onClick={() => void handleRevokeSession(session)} disabled={revokingSessionId === session.id}>{revokingSessionId === session.id ? t.userPanel.revoking : t.userPanel.revokeSession}</button></div>
            </div>)}</div>}
          </>}
        </div>
      </div>}

      {editingUser && <UserEditModal user={editingUser} onSave={(request) => void handleSave(request)} onClose={() => setEditingUser(null)} isSaving={isSaving} error={operationError} t={t.userPanel} />}
    </div>
  );
};

interface UserEditModalProps {
  user: User;
  onSave: (request: UpdateUserRequest) => void;
  onClose: () => void;
  isSaving: boolean;
  error: string | null;
  t: ReturnType<typeof useI18n>["t"]["userPanel"];
}

const UserEditModal = ({ user, onSave, onClose, isSaving, error, t }: UserEditModalProps) => {
  const [email, setEmail] = useState(user.email);
  const [displayName, setDisplayName] = useState(user.displayName ?? "");
  const [role, setRole] = useState<UserRole>(user.role === "Admin" ? "Admin" : "User");

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!email.trim() || !email.includes("@")) return;
    onSave({ email: email.trim(), displayName: displayName.trim(), role });
  };

  return <div className={styles.modalOverlay} onClick={onClose}>
    <div className={styles.modal} onClick={(event) => event.stopPropagation()} role="dialog" aria-modal="true">
      <div className={styles.modalHeader}><h2>{t.editTitle}</h2><button type="button" className={styles.closeButton} onClick={onClose} aria-label={t.close}>×</button></div>
      {error && <div className={styles.error} role="alert">{error}</div>}
      <form onSubmit={handleSubmit} className={styles.form}>
        <label>{t.emailLabel}<input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label>
        <label>{t.nameLabel}<input type="text" value={displayName} onChange={(event) => setDisplayName(event.target.value)} /></label>
        <label>{t.roleLabel}<select value={role} onChange={(event) => setRole(event.target.value as UserRole)}><option value="User">{t.userRole}</option><option value="Admin">{t.adminRole}</option></select></label>
        {email.trim() && !email.includes("@") && <p className={styles.validationError}>{t.invalidEmail}</p>}
        <div className={styles.modalActions}><button type="button" className={styles.secondaryButton} onClick={onClose} disabled={isSaving}>{t.cancel}</button><button type="submit" className={styles.primaryButton} disabled={isSaving || !email.trim() || !email.includes("@")}>{isSaving ? t.saving : t.save}</button></div>
      </form>
    </div>
  </div>;
};

export default UserPanel;
