export interface TroopsState {
  id: string;
  scheduleId: string;
  villageCount: number;
  playerCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface UploadTroopsStateRequest {
  rawData: string;
}
