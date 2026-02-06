import { useState } from "react";
import { ScheduleType } from "../types/schedule";
import { TroopsStateManager } from "./TroopsStateManager";
import { ReconnaissanceSettings } from "./ReconnaissanceSettings";
import styles from "./ScheduleTabs.module.css";

interface ScheduleTabsProps {
  scheduleId: string;
  scheduleType: ScheduleType;
}

type TabType = "troops" | "reconnaissance";

export const ScheduleTabs = ({
  scheduleId,
  scheduleType,
}: ScheduleTabsProps) => {
  const [activeTab, setActiveTab] = useState<TabType>("troops");

  const tabs: { id: TabType; label: string; visible: boolean }[] = [
    { id: "troops", label: "Stan Armii", visible: true },
    {
      id: "reconnaissance",
      label: "Ustawienia Zwiadowcze",
      visible: scheduleType === ScheduleType.Reconnaissance,
    },
  ];

  const visibleTabs = tabs.filter((tab) => tab.visible);

  return (
    <div className={styles.tabsContainer}>
      <div className={styles.tabsHeader}>
        {visibleTabs.map((tab) => (
          <button
            key={tab.id}
            className={`${styles.tab} ${activeTab === tab.id ? styles.active : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className={styles.tabContent}>
        {activeTab === "troops" && (
          <TroopsStateManager scheduleId={scheduleId} />
        )}
        {activeTab === "reconnaissance" &&
          scheduleType === ScheduleType.Reconnaissance && (
            <ReconnaissanceSettings scheduleId={scheduleId} />
          )}
      </div>
    </div>
  );
};
