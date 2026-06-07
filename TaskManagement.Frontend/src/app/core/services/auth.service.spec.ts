import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  afterEach(() => localStorage.clear());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('hasToken() is false when nothing is stored', () => {
    expect(service.hasToken()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('setToken() stores the token and getToken() returns it', () => {
    service.setToken('abc.def.ghi');
    expect(service.getToken()).toBe('abc.def.ghi');
    expect(service.hasToken()).toBeTrue();
  });

  it('isAuthenticated$ emits true after setToken()', (done) => {
    service.setToken('abc.def.ghi');
    service.isAuthenticated$.subscribe((value) => {
      expect(value).toBeTrue();
      done();
    });
  });

  it('logout() clears the token and isAuthenticated$ emits false', (done) => {
    service.setToken('abc.def.ghi');
    service.logout();
    expect(service.hasToken()).toBeFalse();
    service.isAuthenticated$.subscribe((value) => {
      expect(value).toBeFalse();
      done();
    });
  });

  it('getTokenClaims() decodes the JWT payload', () => {
    const payload = btoa(JSON.stringify({ name: 'Hans', oid: '123' }));
    service.setToken(`header.${payload}.signature`);

    const claims = service.getTokenClaims();

    expect(claims).not.toBeNull();
    expect(claims.name).toBe('Hans');
    expect(claims.oid).toBe('123');
  });

  it('getTokenClaims() returns null for a malformed token', () => {
    service.setToken('not-a-jwt');
    expect(service.getTokenClaims()).toBeNull();
  });
});
