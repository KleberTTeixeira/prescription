import { Routes } from '@angular/router';
import { professionalSelectedGuard } from './presentation/guards/professional-selected.guard';
import { Shell } from './presentation/layout/shell/shell';
import { Favorites } from './presentation/pages/favorites/favorites';
import { PrescriptionForm } from './presentation/pages/prescription-form/prescription-form';
import { PrescriptionPrint } from './presentation/pages/prescription-print/prescription-print';
import { PrescriptionView } from './presentation/pages/prescription-view/prescription-view';
import { Prescriptions } from './presentation/pages/prescriptions/prescriptions';
import { ProfessionalSelection } from './presentation/pages/professional-selection/professional-selection';
import { Templates } from './presentation/pages/templates/templates';

export const routes: Routes = [
  { path: 'profissional', component: ProfessionalSelection, title: 'Escolha do profissional' },
  {
    path: '',
    component: Shell,
    canActivate: [professionalSelectedGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'receitas' },
      { path: 'receitas', component: Prescriptions, title: 'Receitas' },
      { path: 'receitas/nova', component: PrescriptionForm, title: 'Nova receita' },
      { path: 'receitas/:id', component: PrescriptionView, title: 'Receita' },
      { path: 'receitas/:id/editar', component: PrescriptionForm, title: 'Editar receita' },
      { path: 'receitas/:id/imprimir', component: PrescriptionPrint, title: 'Imprimir receita' },
      { path: 'textos-padrao', component: Templates, title: 'Textos padrão' },
      { path: 'favoritos', component: Favorites, title: 'Favoritos' }
    ]
  },
  { path: '**', redirectTo: 'receitas' }
];
