import { useState, useEffect, useCallback } from "react";
import { attackCommandsService } from "../services/attackCommandsService";
import type {
  MainActionStats as MainActionStatsData,
  CommandsPerPlayer,
  CommandsPerDeparturePeriod,
} from "../types/mainActionStats";
import styles from "./MainActionStats.module.css";

interface MainActionStatsProps {
  scheduleId: string;
}

const COMMAND_TYPE_LABELS: Record<string, string> = {
  Off: "OFF",
  FakeOffensive: "Fejk OFF",
  FakeDefensive: "Fejk DEFF",
  Catapults: "Burzenie",
  NobleWithDeff: "Szlachcic + zagr. deffa",
  NobleWithFullOff: "Szlachcic + pełny off",
  NobleWithHalfOff: "Szlachcic + połowa offa",
  NobleWithFakeOff: "Szlachcic + fejk off",
  NobleWithFakeDeff: "Szlachcic + fejk deff",
};

const ALL_FILTER = "__all__";

function getLabel(type: string): string {
  return COMMAND_TYPE_LABELS[type] ?? type;
}

function extractError(err: unknown): string {
  if (err && typeof err === "object" && "response" in err) {
    const axiosErr = err as {
      response?: { status?: number; data?: { error?: string } };
    };
    if (axiosErr.response?.status === 404) {
      return 'Nie wygenerowano jeszcze komend dla tej rozpiski. Przejdź do zakładki "Generuj Akcje" i wygeneruj akcję.';
    }
    if (axiosErr.response?.data?.error) {
      return axiosErr.response.data.error;
    }
  }
  return "Nie udało się załadować statystyk akcji";
}

// ── Player table ──────────────────────────────────────────────────────────────

interface PlayerTableProps {
  rows: CommandsPerPlayer[];
  selectedType: string;
  allTypes: string[];
}

function PlayerTable({ rows, selectedType, allTypes }: PlayerTableProps) {
  const filtered =
    selectedType === ALL_FILTER
      ? rows
      : rows.filter((r) => (r.countByType[selectedType] ?? 0) > 0);

  const getCount = (row: CommandsPerPlayer) =>
    selectedType === ALL_FILTER
      ? row.totalCount
      : (row.countByType[selectedType] ?? 0);

  const sorted = [...filtered].sort((a, b) => getCount(b) - getCount(a));

  if (sorted.length === 0) {
    return (
      <div className={styles.noData}>Brak danych dla wybranego filtra</div>
    );
  }

  const showTypeTags = selectedType === ALL_FILTER;

  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <thead>
          <tr>
            <th>Gracz</th>
            <th className={styles.right}>Liczba Komend</th>
            {showTypeTags && <th>Rozkład Typów</th>}
          </tr>
        </thead>
        <tbody>
          {sorted.map((row) => (
            <tr key={row.playerId}>
              <td className={styles.playerName}>{row.playerName}</td>
              <td className={styles.right}>
                <span className={styles.countBadge}>{getCount(row)}</span>
              </td>
              {showTypeTags && (
                <td>
                  <div className={styles.typeTags}>
                    {allTypes
                      .filter((t) => (row.countByType[t] ?? 0) > 0)
                      .map((t) => (
                        <span key={t} className={styles.typeTag}>
                          {getLabel(t)}: {row.countByType[t]}
                        </span>
                      ))}
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ── Hour bar chart ────────────────────────────────────────────────────────────

interface HourChartProps {
  data: MainActionStatsData["commandsPerArrivalHour"];
  selectedType: string;
}

function HourChart({ data, selectedType }: HourChartProps) {
  const counts = data.map((h) =>
    selectedType === ALL_FILTER
      ? h.totalCount
      : (h.countByType[selectedType] ?? 0),
  );
  const maxCount = Math.max(...counts, 1);

  // Build a full 24-hour grid so gaps are visible.
  const hourMap = new Map(data.map((h) => [h.hour, h]));
  const presentHours = data.map((h) => h.hour);
  const minHour = Math.min(...presentHours, 0);
  const maxHour = Math.max(...presentHours, 23);

  const hoursRange = Array.from(
    { length: maxHour - minHour + 1 },
    (_, i) => minHour + i,
  );

  if (hoursRange.length === 0) {
    return <div className={styles.barEmptyHint}>Brak danych</div>;
  }

  return (
    <div className={styles.chartWrapper}>
      <div className={styles.barsGrid}>
        {hoursRange.map((h) => {
          const entry = hourMap.get(h);
          const count =
            entry === undefined
              ? 0
              : selectedType === ALL_FILTER
                ? entry.totalCount
                : (entry.countByType[selectedType] ?? 0);
          const heightPct = maxCount > 0 ? (count / maxCount) * 100 : 0;

          return (
            <div
              key={h}
              className={styles.barColumnInner}
              title={`${h}:00 – ${count} komend`}
            >
              <div
                className={styles.bar}
                style={{ height: `${Math.max(heightPct, count > 0 ? 2 : 0)}%` }}
              />
              <span className={styles.barLabel}>{h}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Departure period bar chart ───────────────────────────────────────────────

interface DeparturePeriodChartProps {
  rows: CommandsPerDeparturePeriod[];
  selectedType: string;
  allTypes: string[];
}

function DeparturePeriodChart({
  rows,
  selectedType,
  allTypes,
}: DeparturePeriodChartProps) {
  if (rows.length === 0) {
    return <div className={styles.barEmptyHint}>Brak danych</div>;
  }

  const getCount = (row: CommandsPerDeparturePeriod) =>
    selectedType === ALL_FILTER
      ? row.totalCount
      : (row.countByType[selectedType] ?? 0);

  const filtered =
    selectedType === ALL_FILTER
      ? rows
      : rows.filter((r) => getCount(r) > 0);

  if (filtered.length === 0) {
    return <div className={styles.barEmptyHint}>Brak danych dla wybranego filtra</div>;
  }

  const maxCount = Math.max(...filtered.map(getCount), 1);

  const formatBarLabel = (row: CommandsPerDeparturePeriod) => {
    const d = new Date(row.date);
    const day = String(d.getUTCDate()).padStart(2, "0");
    const month = String(d.getUTCMonth() + 1).padStart(2, "0");
    return `${day}.${month}\n${row.slotStart}–${row.slotStart + 8}`;
  };

  const showTypeTags = selectedType === ALL_FILTER;

  return (
    <div className={styles.chartWrapper}>
      <div className={styles.departureBarsGrid}>
        {rows.map((row) => {
          const count = getCount(row);
          const heightPct = maxCount > 0 ? (count / maxCount) * 100 : 0;
          const d = new Date(row.date);
          const day = String(d.getUTCDate()).padStart(2, "0");
          const month = String(d.getUTCMonth() + 1).padStart(2, "0");
          const tooltip = `${day}.${month} ${row.slotStart}:00–${row.slotStart + 8}:00: ${count} komend`
            + (showTypeTags
              ? "\n" + allTypes.filter(t => (row.countByType[t] ?? 0) > 0).map(t => `${getLabel(t)}: ${row.countByType[t]}`).join(", ")
              : "");

          return (
            <div
              key={`${row.date}-${row.slotStart}`}
              className={styles.departurePeriodColumn}
              title={tooltip}
            >
              <span className={styles.departureBarCount}>
                {count > 0 ? count : ""}
              </span>
              <div
                className={styles.departurePeriodBar}
                style={{ height: `${Math.max(heightPct, count > 0 ? 3 : 0)}%` }}
              />
              <span className={styles.departurePeriodLabel}>
                {formatBarLabel(row)}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Departure period table ────────────────────────────────────────────────────

function formatPeriodLabel(row: CommandsPerDeparturePeriod): string {
  const d = new Date(row.date);
  const day = String(d.getUTCDate()).padStart(2, "0");
  const month = String(d.getUTCMonth() + 1).padStart(2, "0");
  return `${day}.${month} ${row.slotStart}:00–${row.slotStart + 8}:00`;
}

interface DeparturePeriodTableProps {
  rows: CommandsPerDeparturePeriod[];
  selectedType: string;
  allTypes: string[];
}

function DeparturePeriodTable({
  rows,
  selectedType,
  allTypes,
}: DeparturePeriodTableProps) {
  const filtered =
    selectedType === ALL_FILTER
      ? rows
      : rows.filter((r) => (r.countByType[selectedType] ?? 0) > 0);

  const getCount = (row: CommandsPerDeparturePeriod) =>
    selectedType === ALL_FILTER
      ? row.totalCount
      : (row.countByType[selectedType] ?? 0);

  if (filtered.length === 0) {
    return (
      <div className={styles.noData}>Brak danych dla wybranego filtra</div>
    );
  }

  const showTypeTags = selectedType === ALL_FILTER;

  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <thead>
          <tr>
            <th>Przedział czasu</th>
            <th className={styles.right}>Liczba Komend</th>
            {showTypeTags && <th>Rozkład Typów</th>}
          </tr>
        </thead>
        <tbody>
          {filtered.map((row) => (
            <tr key={`${row.date}-${row.slotStart}`}>
              <td className={styles.playerName}>{formatPeriodLabel(row)}</td>
              <td className={styles.right}>
                <span className={styles.countBadge}>{getCount(row)}</span>
              </td>
              {showTypeTags && (
                <td>
                  <div className={styles.typeTags}>
                    {allTypes
                      .filter((t) => (row.countByType[t] ?? 0) > 0)
                      .map((t) => (
                        <span key={t} className={styles.typeTag}>
                          {getLabel(t)}: {row.countByType[t]}
                        </span>
                      ))}
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ── Type filter ───────────────────────────────────────────────────────────────

interface TypeFilterProps {
  allTypes: string[];
  selected: string;
  onChange: (type: string) => void;
}

function TypeFilter({ allTypes, selected, onChange }: TypeFilterProps) {
  return (
    <div className={styles.filterGroup}>
      <button
        className={`${styles.filterBtn} ${selected === ALL_FILTER ? styles.filterActive : ""}`}
        onClick={() => onChange(ALL_FILTER)}
      >
        Wszystkie
      </button>
      {allTypes.map((t) => (
        <button
          key={t}
          className={`${styles.filterBtn} ${selected === t ? styles.filterActive : ""}`}
          onClick={() => onChange(t)}
        >
          {getLabel(t)}
        </button>
      ))}
    </div>
  );
}

// ── Root component ────────────────────────────────────────────────────────────

export const MainActionStats = ({ scheduleId }: MainActionStatsProps) => {
  const [stats, setStats] = useState<MainActionStatsData | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Independent type filters per section
  const [enemyFilter, setEnemyFilter] = useState<string>(ALL_FILTER);
  const [sourceFilter, setSourceFilter] = useState<string>(ALL_FILTER);
  const [hourFilter, setHourFilter] = useState<string>(ALL_FILTER);
  const [departureFilter, setDepartureFilter] = useState<string>(ALL_FILTER);

  const loadStats = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await attackCommandsService.getMainActionStats(scheduleId);
      setStats(data);
    } catch (err: unknown) {
      setError(extractError(err));
    } finally {
      setIsLoading(false);
    }
  }, [scheduleId]);

  useEffect(() => {
    loadStats();
  }, [loadStats]);

  if (isLoading) {
    return (
      <div className={styles.container}>
        <h3>Statystyki Akcji</h3>
        <div className={styles.loading}>Ładowanie statystyk...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.container}>
        <h3>Statystyki Akcji</h3>
        <div className={styles.error}>{error}</div>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className={styles.container}>
        <h3>Statystyki Akcji</h3>
        <div className={styles.noData}>Brak danych</div>
      </div>
    );
  }

  // Collect all command types across the whole action (sorted by total occurrence).
  const typeCountMap: Record<string, number> = {};
  for (const entry of stats.commandsPerArrivalHour) {
    for (const [t, c] of Object.entries(entry.countByType)) {
      typeCountMap[t] = (typeCountMap[t] ?? 0) + c;
    }
  }
  const allTypes = Object.entries(typeCountMap)
    .sort((a, b) => b[1] - a[1])
    .map(([t]) => t);

  const uniqueEnemyPlayers = stats.commandsPerEnemyPlayer.length;
  const uniqueSourcePlayers = stats.commandsPerSourcePlayer.length;

  return (
    <div className={styles.container}>
      <h3>Statystyki Akcji</h3>
      <p className={styles.subtitle}>
        Wygenerowano łącznie {stats.totalCommands} komend
      </p>

      {/* ── Overview cards ── */}
      <div className={styles.overviewBanner}>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{stats.totalCommands}</span>
          <span className={styles.statLabel}>Wszystkich Komend</span>
        </div>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{uniqueEnemyPlayers}</span>
          <span className={styles.statLabel}>Atakowanych Graczy</span>
        </div>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{uniqueSourcePlayers}</span>
          <span className={styles.statLabel}>Wysyłających Graczy</span>
        </div>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{allTypes.length}</span>
          <span className={styles.statLabel}>Typy Komend</span>
        </div>
      </div>

      {/* ── Enemy players ── */}
      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <h4 className={styles.sectionTitle}>Komendy na Wrogich Graczy</h4>
          <TypeFilter
            allTypes={allTypes}
            selected={enemyFilter}
            onChange={setEnemyFilter}
          />
        </div>
        <PlayerTable
          rows={stats.commandsPerEnemyPlayer}
          selectedType={enemyFilter}
          allTypes={allTypes}
        />
      </div>

      {/* ── Source players ── */}
      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <h4 className={styles.sectionTitle}>
            Komendy do Wysłania przez Graczy
          </h4>
          <TypeFilter
            allTypes={allTypes}
            selected={sourceFilter}
            onChange={setSourceFilter}
          />
        </div>
        <PlayerTable
          rows={stats.commandsPerSourcePlayer}
          selectedType={sourceFilter}
          allTypes={allTypes}
        />
      </div>

      {/* ── Hourly distribution ── */}
      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <h4 className={styles.sectionTitle}>Rozkład ataków – godzina wejścia</h4>
          <TypeFilter
            allTypes={allTypes}
            selected={hourFilter}
            onChange={setHourFilter}
          />
        </div>
        <HourChart
          data={stats.commandsPerArrivalHour}
          selectedType={hourFilter}
        />
        <div className={styles.tableWrapper} style={{ marginTop: "1rem" }}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Godzina</th>
                <th className={styles.right}>Liczba Komend</th>
                {hourFilter === ALL_FILTER && <th>Rozkład Typów</th>}
              </tr>
            </thead>
            <tbody>
              {stats.commandsPerArrivalHour
                .filter((h) =>
                  hourFilter === ALL_FILTER
                    ? true
                    : (h.countByType[hourFilter] ?? 0) > 0,
                )
                .map((h) => {
                  const count =
                    hourFilter === ALL_FILTER
                      ? h.totalCount
                      : (h.countByType[hourFilter] ?? 0);
                  return (
                    <tr key={h.hour}>
                      <td>{String(h.hour).padStart(2, "0")}:00</td>
                      <td className={styles.right}>
                        <span className={styles.countBadge}>{count}</span>
                      </td>
                      {hourFilter === ALL_FILTER && (
                        <td>
                          <div className={styles.typeTags}>
                            {allTypes
                              .filter((t) => (h.countByType[t] ?? 0) > 0)
                              .map((t) => (
                                <span key={t} className={styles.typeTag}>
                                  {getLabel(t)}: {h.countByType[t]}
                                </span>
                              ))}
                          </div>
                        </td>
                      )}
                    </tr>
                  );
                })}
            </tbody>
          </table>
        </div>
      </div>

      {/* ── Departure period distribution ── */}
      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <h4 className={styles.sectionTitle}>Rozkład ataków – godzina wysłania (przedziały 8h)</h4>
          <TypeFilter
            allTypes={allTypes}
            selected={departureFilter}
            onChange={setDepartureFilter}
          />
        </div>
        <DeparturePeriodChart
          rows={stats.commandsPerDeparturePeriod}
          selectedType={departureFilter}
          allTypes={allTypes}
        />
        <DeparturePeriodTable
          rows={stats.commandsPerDeparturePeriod}
          selectedType={departureFilter}
          allTypes={allTypes}
        />
      </div>
    </div>
  );
};
