export class User {
    id!: string;
    password!: string;
    name!: string;
    email!: string;
    document!: string;
    document_type!: string;
    type!: string;
    gender!: string;
    birthdate!: Date;
    line_1!: string;
    line_2!: string;
    zip_code!: string;
    city!: string;
    state!: string;
    country!: string;
    foto!: string;
    fotoUpload!: string;
}

// document --> Numeros, máximo 16caracteres para cpf e cnpj e 50 para passaporte
// document_type --> CPF / CNPJ / PASSPORT
// type --> individual para PF e company para PJ
// gender --> male / female

export class ResetPassword {
    username!: string;
}

export class ResetPasswordUser {
    username!: string;
    newPassord!: string;
    confirmPassword!: string;
    token!: string;
}

export class PagarmeCard {
    number!: string;
    holder_name!: string;
    holder_document!: string;
    exp_month!: string;
    exp_year!: string;
    cvv!: string;
    brand!: string;
    billing_address!: PagarmeCardBillingAddress;
}

export class ListCard {
    card_id!: string;
    first_six_digits!: string;
    last_four_digits!: string;
    exp_month!: string;
    exp_year!: string;
    status!: string;
}

export class PagarmeCardBillingAddress {
    line_1!: string;
    city!: string;
    state!: string;
    cuntry!: string;
    zip_code!: string;
}

export class PagarmeResponse<T> {
    data!: T[];
    paging!: Paging;
}

export class Paging {
    total!: number;
}

export class PagarmeCardResponse {
    id!: string;
    first_six_digits!: string;
    last_four_digits!: string;
    brand!: string;
    holder_name!: string;
    holder_document!: string;
    exp_month!: string;
    exp_year!: string;
    status!: string;
    type!: string;
    label!: string;
    created_at!: string;
    updated_at!: string;
    billing_address!: PagarmeCardBillingAddressResponse;
}

export class PagarmeCardBillingAddressResponse {
    line_1!: string;
    line_2!: string;
    state!: string;
    city!: string;
    country!: string;
    zip_code!: string;
}

export class JWToken {
    accessToken!: string;
    expiresIn!: number;
    userToken!: UserToken;
}

export class UserToken {
    email!: string;
    id!: string;
    nome!: string;
    claims!: ClaimUserToken[];
}

export class ClaimUserToken {
    type!: string;
    value!: string;
}