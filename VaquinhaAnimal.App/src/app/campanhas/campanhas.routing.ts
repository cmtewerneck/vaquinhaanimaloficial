import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CampanhasComponent } from './campanhas.component';
import { ListarTodasComponent } from './listar-todas/listar-todas.component';
import { CriarComponent } from './criar/criar.component';
import { CampanhaGuard } from './campanha.guard';
import { MinhasCampanhasComponent } from './minhas-campanhas/minhas-campanhas.component';

const campanhasRouterConfig: Routes = [
    {
        path: '', component: CampanhasComponent,
        children: [
            { path: 'listar-todas', component: ListarTodasComponent },
            {
                path: 'criar', component: CriarComponent,
                canActivate: [CampanhaGuard],
                canDeactivate: [CampanhaGuard]
            },
            { 
                path: 'minhas-campanhas', component: MinhasCampanhasComponent,
                canActivate: [CampanhaGuard]
            },
            { path: '**', redirectTo: 'listar-todas', pathMatch: 'full' }
        ]
    }
];

@NgModule({
    imports: [
        RouterModule.forChild(campanhasRouterConfig)
    ],
    exports: [RouterModule]
})
export class CampanhasRoutingModule { }