%define		debug_package %{nil}
#
# spec file for package AtspiUiaSource
#

Name:           atspiuiasource
Version:        1.8.90
Release:        1
License:        MIT
Group:          System/Libraries
URL:            http://www.mono-project.com/Accessibility
Source0:        http://ftp.novell.com/pub/mono/sources/mono-uia/%{name}-%{version}.tar.bz2
BuildRoot:      %{_tmppath}/%{name}-%{version}-%{release}-root-%(%{__id_u} -n)
Summary:        At-spi uia source
BuildRequires:	at-spi-sharp-devel
BuildRequires:	glib-sharp2 >= 2.12.8
BuildRequires:	mono-devel >= 2.6
BuildRequires:	mono-uia-devel >= 1.8.90
BuildRequires:	pkg-config

%description
At-spi uia source client side

%prep
%setup -q

%build
%configure --disable-tests
#make %{?_smp_mflags}
make

%install
rm -rf %{buildroot}
make DESTDIR=%{buildroot} install

%clean
rm -rf %{buildroot}

%files
%defattr(-,root,root)
%dir %{_prefix}/lib/mono/gac/AtspiUiaSource
%{_prefix}/lib/mono/gac/AtspiUiaSource/*
%dir %{_libdir}/atspiuiasource
%{_libdir}/atspiuiasource/*

%changelog
* Mon Nov 30 2009 Stephen Shaw <sshaw@decriptor.com> = 1.8.90-1
- Initial RPM