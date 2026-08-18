export interface Item {
  id: number;
  name: string;
  description: string;
  category: string;
  quantity: number;
  unit: string;
  minQuantity: number;
  donor: string;
  entryDate: Date;
  expiryDate: Date | null;
}

export interface Movement {
  id: number;
  itemId: number;
  itemName: string;
  type: 'entry' | 'exit';
  quantity: number;
  date: Date;
  description: string;
}

export interface NotificationConfig {
  onEntry: boolean;
  onExit: boolean;
  onExpiry: boolean;
  emails: string[];
}