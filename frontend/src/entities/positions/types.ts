export type GetPositionDto = {
  id: string;
  speciality: string;
  direction: string;
  isActive: boolean;
};

export type PositionNameRequest = {
  speciality: string;
  direction: string;
};

export type CreatePositionRequest = {
  positionName: PositionNameRequest;
  description?: string;
  departmentIds: string[];
};

export type RenamePositionRequest = {
  positionName: PositionNameRequest;
};
