import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomepageComponent } from './homepage/homepage.component';
import { CampanhasComponent } from './campanhas/campanhas.component';
import { BlogComponent } from './blog/blog.component';
import { ContatoComponent } from './contato/contato.component';

const routes: Routes = [
  { path: 'homepage', redirectTo: '', pathMatch: 'full' },
  { path: '', component: HomepageComponent },
  { path: 'auth',
            loadChildren: () => import('./auth/auth.module')
            .then(m => m.AuthModule)
  },
  { path: 'campanhas',
            loadChildren: () => import('./campanhas/campanhas.module')
            .then(m => m.CampanhasModule)
  },
  { path: 'contato',component: ContatoComponent },
  { path: 'artigos',component: BlogComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {anchorScrolling: 'enabled'})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
