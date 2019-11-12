import { FileType } from './file';
import { CompanyRelationshipType } from './company';
import { MongoDocumentType } from './mongoDocument';

export interface AccountType extends MongoDocumentType {
    email: string;
    lastname: string;
    password: string;
    firstname: string;
    activated: boolean;
    expire: string | null;
    outlookAccountId?: string;
    salesforceAccountId?: string;
    profilePicture: FileType | null;
    company: CompanyRelationshipType;
}
