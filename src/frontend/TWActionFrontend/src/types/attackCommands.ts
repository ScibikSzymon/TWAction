export interface TimeWindow {
  minDepartureTime: string;
  maxDepartureTime: string;
  minArrivalTime: string;
  maxArrivalTime: string;
}

export interface VillageSmall {
  id: number;
  x: number;
  y: number;
  playerId: number;
}

export interface AttackCommand {
  id: string;
  timeWindow: TimeWindow;
  source: VillageSmall;
  destination: VillageSmall;
  commandType: string;
  createdAt: string;
}
