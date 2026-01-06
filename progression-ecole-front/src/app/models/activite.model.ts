export interface Activite {
  id: string;
  libelleTresCourt: string;
  libelleCourt: string;
  libelleLong: string;
  categorieId: string;
  ordre: number;
  estRegroupement: boolean;
  parentId?: string | null;
  estParametrable?: boolean;
  modeleLibelle?: string;
  nomsParametres?: string[];
}

export interface ActivitePersonnalisee {
  id: string;
  eleveId: string;
  activiteId: string;
  periode: string;
  valeursParametres: { [key: string]: string };
  dateCreation: Date;
}
