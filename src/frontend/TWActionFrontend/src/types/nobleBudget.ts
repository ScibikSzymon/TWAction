export interface NobleBudget {
  id: string;
  scheduleId: string;
  playerId: number;
  budget: number;
  createdAt: string;
  updatedAt: string;
}

export interface PlayerNobleStats {
  playerId: number;
  playerName: string;
  totalNobles: number;
}

export interface SaveNobleBudgetsRequest {
  playerBudgets: PlayerBudgetItem[];
}

export interface PlayerBudgetItem {
  playerId: number;
  budget: number;
}
