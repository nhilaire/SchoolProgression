export interface Activite {
  id: string;
  libelleCourt: string;
  libelleLong: string;
  categorieId: string;
  ordre: number;
  estRegroupement: boolean;
  parentId?: string | null;
}
