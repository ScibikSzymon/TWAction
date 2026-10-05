export interface TemplateWave {
  minTime: string; // "HH:mm:ss" format from backend
  maxTime: string; // "HH:mm:ss" format from backend
  commandNumber: number;
  commandType: string;
}

export interface TargetTemplate {
  id: string;
  userId: string | null;
  name: string;
  isDefault: boolean;
  waves: TemplateWave[];
}

export interface CreateTargetTemplateRequest {
  name: string;
  waves: TemplateWave[];
}

export interface UpdateTargetTemplateRequest {
  name: string;
  waves: TemplateWave[];
}

/** All valid command type identifiers mirroring CommandTypeConstants on the backend. */
export const CommandTypes = {
  Off: "Off",
  FakeOffensive: "FakeOffensive",
  FakeDefensive: "FakeDefensive",
  Catapults: "Catapults",
  NobleWithDeff: "NobleWithDeff",
  NobleWithFullOff: "NobleWithFullOff",
  NobleWithHalfOff: "NobleWithHalfOff",
  NobleWithQuarterOffensive: "NobleWithQuarterOffensive",
  NobleWith150Axes: "NobleWith150Axes",
  NobleWith100HeavyCavalry: "NobleWith100HeavyCavalry",
  RandomNoble: "RandomNoble",
} as const;

export type CommandType = (typeof CommandTypes)[keyof typeof CommandTypes];

/** Human-readable Polish labels for each command type. */
export const commandTypeLabels: Record<string, string> = {
  Off: "OFF (atak offowy)",
  FakeOffensive: "Fejk OFF",
  FakeDefensive: "Fejk DEFF",
  Catapults: "Burzenie (katapulty)",
  NobleWithDeff: "Szlachcic z zagrodą deffa",
  NobleWithFullOff: "Szlachcic z pełnym offem",
  NobleWithHalfOff: "Szlachcic z połową offa",
  NobleWithQuarterOffensive: "Szlachcic z ćwiartką offa",
  NobleWith150Axes: "Szlachcic + 150 toporów",
  NobleWith100HeavyCavalry: "Szlachcic + 100 CK",
  RandomNoble: "Szlachcic (losowy typ)",
};

export const ALL_COMMAND_TYPES = Object.values(CommandTypes);

/** Creates an empty wave with safe default values for the form editor. */
export function createEmptyWave(): TemplateWave {
  return {
    minTime: "08:00:00",
    maxTime: "09:30:00",
    commandNumber: 1,
    commandType: CommandTypes.Off,
  };
}
