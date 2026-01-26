import { useState, useEffect } from "react";
import { tribesService } from "../services/tribesService";
import type { Tribe, EnemyTribeSnapshot } from "../types/tribe";
import type { WorldType } from "../types/schedule";
import styles from "./TribeSelector.module.css";

interface TribeSelectorProps {
  world: WorldType | null;
  selectedTribes: EnemyTribeSnapshot[];
  onSelectionChange: (tribes: EnemyTribeSnapshot[]) => void;
  onTribesLoaded?: (tribes: EnemyTribeSnapshot[]) => void;
  maxTribes?: number;
}

export const TribeSelector = ({
  world,
  selectedTribes,
  onSelectionChange,
  onTribesLoaded,
  maxTribes = 10,
}: TribeSelectorProps) => {
  const [tribes, setTribes] = useState<Tribe[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    if (world) {
      loadTribes();
    } else {
      setTribes([]);
      setError(null);
    }
  }, [world]);

  const loadTribes = async () => {
    if (!world) return;

    setLoading(true);
    setError(null);

    try {
      const data = await tribesService.getTribesForWorld(world);
      setTribes(data);

      // Powiadom rodzica że plemiona zostały załadowane
      if (onTribesLoaded) {
        onTribesLoaded(data);
      }
    } catch (err) {
      console.error("Failed to load tribes:", err);
      setError("Nie udało się pobrać listy plemion");
    } finally {
      setLoading(false);
    }
  };

  const toggleTribe = (tribe: Tribe) => {
    const isSelected = selectedTribes.some(
      (t) => t.tribalWarsId === tribe.tribalWarsId,
    );

    if (isSelected) {
      onSelectionChange(
        selectedTribes.filter((t) => t.tribalWarsId !== tribe.tribalWarsId),
      );
    } else {
      if (selectedTribes.length >= maxTribes) {
        return;
      }

      onSelectionChange([
        ...selectedTribes,
        {
          tribalWarsId: tribe.tribalWarsId,
          name: tribe.name,
          short: tribe.short,
          villagesCount: tribe.villagesCount,
        },
      ]);
    }
  };

  const filteredTribes = tribes.filter(
    (t) =>
      t.short.toLowerCase().includes(searchTerm.toLowerCase()) ||
      t.name.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  if (!world) {
    return (
      <div className={styles.container}>
        <div className={styles.placeholder}>Najpierw wybierz świat</div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h3>Wrogie plemiona</h3>
        <button
          type="button"
          onClick={loadTribes}
          disabled={loading}
          className={styles.refreshBtn}
        >
          {loading ? "Odświeżanie..." : "Odśwież"}
        </button>
      </div>

      {error && <div className={styles.error}>{error}</div>}

      {!error && (
        <>
          <input
            type="text"
            placeholder="Szukaj po tagu lub nazwie..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className={styles.search}
          />

          {loading ? (
            <div className={styles.loading}>Ładowanie plemion...</div>
          ) : (
            <>
              <div className={styles.tribeList}>
                {filteredTribes.map((tribe) => {
                  const isSelected = selectedTribes.some(
                    (t) => t.tribalWarsId === tribe.tribalWarsId,
                  );
                  const isDisabled =
                    !isSelected && selectedTribes.length >= maxTribes;

                  return (
                    <label
                      key={tribe.tribalWarsId}
                      className={styles.tribeItem}
                      style={{
                        opacity: isDisabled ? 0.5 : 1,
                        cursor: isDisabled ? "not-allowed" : "pointer",
                      }}
                    >
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={() => toggleTribe(tribe)}
                        disabled={isDisabled}
                      />
                      <div className={styles.tribeInfo}>
                        <span className={styles.tag}>[{tribe.short}]</span>
                        <span className={styles.name}>{tribe.name}</span>
                        <span className={styles.meta}>
                          {tribe.villagesCount} wiosek
                        </span>
                      </div>
                    </label>
                  );
                })}
              </div>

              <div className={styles.summary}>
                <span>
                  Wybrano:{" "}
                  <span
                    className={
                      selectedTribes.length > maxTribes
                        ? styles.limitExceeded
                        : styles.count
                    }
                  >
                    {selectedTribes.length}
                  </span>{" "}
                  / {maxTribes}
                </span>
                {selectedTribes.length > 0 && (
                  <span className={styles.limit}>
                    {selectedTribes.map((t) => t.short).join(", ")}
                  </span>
                )}
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
};
