export const WorldType = {
  pl218: "pl218",
  pl219: "pl219",
  pl220: "pl220",
  pl221: "pl221",
  pl222: "pl222",
  pl223: "pl223",
} as const;

export type WorldType = (typeof WorldType)[keyof typeof WorldType];

export const ScheduleType = {
  Fake: "Fake",
  Reconnaissance: "Reconnaissance",
  Main: "Main",
} as const;

export type ScheduleType = (typeof ScheduleType)[keyof typeof ScheduleType];

export interface Schedule {
  id: string;
  userId: string;
  name: string;
  creationDate: string;
  world: WorldType;
  scheduleType: ScheduleType;
}

export interface CreateScheduleRequest {
  userId: string;
  name: string;
  world: string;
  scheduleType: string;
}

export interface UpdateScheduleRequest {
  name: string;
  world: string;
  scheduleType: string;
}
