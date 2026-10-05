export interface CommandsPerPlayer {
  playerId: number;
  playerName: string;
  totalCount: number;
  countByType: Record<string, number>;
}

export interface CommandsPerHour {
  hour: number;
  totalCount: number;
  countByType: Record<string, number>;
}

export interface CommandsPerDeparturePeriod {
  /** ISO date string, date-only part (e.g. "2025-06-07") */
  date: string;
  /** 0, 8, or 16 */
  slotStart: number;
  totalCount: number;
  countByType: Record<string, number>;
}

export interface MainActionStats {
  totalCommands: number;
  commandsPerEnemyPlayer: CommandsPerPlayer[];
  commandsPerSourcePlayer: CommandsPerPlayer[];
  commandsPerArrivalHour: CommandsPerHour[];
  commandsPerDeparturePeriod: CommandsPerDeparturePeriod[];
}
